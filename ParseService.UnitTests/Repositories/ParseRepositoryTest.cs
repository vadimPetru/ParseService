using Microsoft.EntityFrameworkCore;
using ParseService.Data;
using ParseService.Models;
using ParseService.Models.Response;
using ParseService.Repository;

namespace ParseService.UnitTests.Repositories
{
    public class ParseRepositoryTest : IDisposable
    {
        private readonly ParseDbContext _dbContext;
        private readonly ParseRepository _repository;

        public ParseRepositoryTest()
        {
            // Для каждого теста создаём уникальную InMemory базу данных
            var options = new DbContextOptionsBuilder<ParseDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ParseDbContext(options);
            _repository = new ParseRepository(_dbContext);
        }

        /// <summary>
        /// 1. Тест: Добавление корректного объявления должно сохранить его в базе.
        /// </summary>
        [Fact]
        public async Task AddAnnouncements_ValidAnnouncement_ItemAdded()
        {
            // Arrange
            var announcement = new AnnouncementItem
            {
                AnnId = 1,
                AnnTitle = "Test Title 1",
                AnnDesc = "Test Description 1",
                AnnUrl = "https://example.com/1",
                Language = "ru_RU",
                CTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // Act
            await _repository.AddAnnouncements(announcement, CancellationToken.None);

            // Assert
            var added = await _dbContext.Announcements.FirstOrDefaultAsync(a => a.AnnId == 1);
            Assert.NotNull(added);
            Assert.Equal("Test Title 1", added.AnnTitle);
        }


        /// <summary>
        /// 2. Тест: После добавления количество записей увеличивается.
        /// </summary>
        [Fact]
        public async Task AddAnnouncements_IncreasesCount()
        {
            // Arrange
            var initialCount = await _dbContext.Announcements.CountAsync();
            var announcement = new AnnouncementItem
            {
                AnnId = 2,
                AnnTitle = "Test Title 2",
                AnnDesc = "Test Description 2",
                Language = "ru_RU",
                AnnUrl = "https://example.com/2",
                CTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // Act
            await _repository.AddAnnouncements(announcement, CancellationToken.None);
            var newCount = await _dbContext.Announcements.CountAsync();

            // Assert
            Assert.Equal(initialCount + 1, newCount);
        }

        /// <summary>
        /// 3. Тест: Попытка добавить null должна выбросить исключение.
        /// </summary>
        [Fact]
        public async Task AddAnnouncements_NullAnnouncement_ThrowsException()
        {
            // Arrange
            AnnouncementItem announcement = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _repository.AddAnnouncements(announcement, CancellationToken.None));
        }

        /// <summary>
        /// 4. Тест: Если CancellationToken отменён, метод должен выбросить OperationCanceledException.
        /// </summary>
        [Fact]
        public async Task AddAnnouncements_CancelledToken_ThrowsOperationCanceledException()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();
            var announcement = new AnnouncementItem
            {
                AnnId = 3,
                AnnTitle = "Test Title 3",
                AnnDesc = "Test Description 3",
                Language = "ru_RU",
                AnnUrl = "https://example.com/3",
                CTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await _repository.AddAnnouncements(announcement, cts.Token));
        }

        /// <summary>
        /// 5. Тест: Последовательное добавление нескольких объявлений – количество записей должно соответствовать.
        /// </summary>
        [Fact]
        public async Task AddAnnouncements_MultipleAdditions_CountMatches()
        {
            // Arrange
            var announcement1 = new AnnouncementItem
            {
                AnnId = 4,
                AnnTitle = "Test Title 4",
                AnnDesc = "Test Description 4",
                AnnUrl = "https://example.com/4",
                Language = "ru_RU",
                CTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            var announcement2 = new AnnouncementItem
            {
                AnnId = 5,
                AnnTitle = "Test Title 5",
                AnnDesc = "Test Description 5",
                Language = "ru_RU",
                AnnUrl = "https://example.com/5",
                CTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // Act
            await _repository.AddAnnouncements(announcement1, CancellationToken.None);
            await _repository.AddAnnouncements(announcement2, CancellationToken.None);

            // Assert
            var count = await _dbContext.Announcements.CountAsync();
            Assert.Equal(2, count);
        }

        /// <summary>
        /// 6. Тест: Все свойства добавленного объявления корректно сохраняются.
        /// </summary>
        [Fact]
        public async Task AddAnnouncements_AnnouncementPropertiesPersist()
        {
            // Arrange
            var announcement = new AnnouncementItem
            {
                AnnId = 6,
                AnnTitle = "Test Title 6",
                AnnDesc = "Test Description 6",
                Language = "ru_RU",
                AnnUrl = "https://example.com/6",
                CTime = 1234567890
            };

            // Act
            await _repository.AddAnnouncements(announcement, CancellationToken.None);
            var added = await _dbContext.Announcements.FirstOrDefaultAsync(a => a.AnnId == 6);

            // Assert
            Assert.NotNull(added);
            Assert.Equal("Test Title 6", added.AnnTitle);
            Assert.Equal("Test Description 6", added.AnnDesc);
            Assert.Equal("https://example.com/6", added.AnnUrl);
            Assert.Equal(1234567890, added.CTime);
        }

        /// <summary>
        /// 7. Тест: Добавление объявления с дублирующимся первичным ключом вызывает исключение.
        /// (Предполагается, что AnnId является первичным ключом)
        /// </summary>
        [Fact]
        public async Task AddAnnouncements_DuplicateKey_ThrowsException()
        {
            // Arrange
            var announcement1 = new AnnouncementItem
            {
                AnnId = 7,
                AnnTitle = "Test Title 7",
                AnnDesc = "Test Description 7",
                Language = "ru_RU",
                AnnUrl = "https://example.com/7",
                CTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            var announcementDuplicate = new AnnouncementItem
            {
                AnnId = 7, // Дублирующий AnnId
                AnnTitle = "Duplicate Title",
                AnnDesc = "Duplicate Description",
                AnnUrl = "https://example.com/duplicate",
                CTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // Act
            await _repository.AddAnnouncements(announcement1, CancellationToken.None);
            // Assert – при попытке добавить дублирующий ключ выбрасывается исключение.
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _repository.AddAnnouncements(announcementDuplicate, CancellationToken.None));
        }

        /// <summary>
        /// 8. Тест: Добавление объявления с пустым заголовком сохраняется (если бизнес-логика это допускает).
        /// </summary>
        [Fact]
        public async Task AddAnnouncements_EmptyTitle_ItemAdded()
        {
            // Arrange
            var announcement = new AnnouncementItem
            {
                AnnId = 8,
                AnnTitle = "", // Пустой заголовок
                AnnDesc = "Test Description 8",
                AnnUrl = "https://example.com/8",
                Language = "ru_RU",
                CTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // Act
            await _repository.AddAnnouncements(announcement, CancellationToken.None);
            var added = await _dbContext.Announcements.FirstOrDefaultAsync(a => a.AnnId == 8);

            // Assert
            Assert.NotNull(added);
            Assert.Equal("", added.AnnTitle);
        }

        /// <summary>
        /// 9. Тест: Добавление объявления с заголовком, состоящим из пробелов, сохраняется корректно.
        /// </summary>
        [Fact]
        public async Task AddAnnouncements_WhitespaceTitle_ItemAdded()
        {
            // Arrange
            var announcement = new AnnouncementItem
            {
                AnnId = 9,
                AnnTitle = "   ", // Заголовок из пробелов
                AnnDesc = "Test Description 9",
                Language = "ru_RU",
                AnnUrl = "https://example.com/9",
                CTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // Act
            await _repository.AddAnnouncements(announcement, CancellationToken.None);
            var added = await _dbContext.Announcements.FirstOrDefaultAsync(a => a.AnnId == 9);

            // Assert
            Assert.NotNull(added);
            Assert.Equal("   ", added.AnnTitle);
        }

        /// <summary>
        /// 10. Тест: Добавление объявления сохраняет корректное время создания (CTime).
        /// </summary>
        [Fact]
        public async Task AddAnnouncements_SavesTimestampProperly()
        {
            // Arrange
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var announcement = new AnnouncementItem
            {
                AnnId = 10,
                AnnTitle = "Test Title 10",
                AnnDesc = "Test Description 10",
                Language = "ru_RU",
                AnnUrl = "https://example.com/10",
                CTime = timestamp
            };

            // Act
            await _repository.AddAnnouncements(announcement, CancellationToken.None);
            var added = await _dbContext.Announcements.FirstOrDefaultAsync(a => a.AnnId == 10);

            // Assert
            Assert.NotNull(added);
            Assert.Equal(timestamp, added.CTime);
        }



        /// <summary>
        /// Проверяет, что метод корректно обрабатывает токен отмены операции.
        /// </summary>
        [Fact]
        public async Task GetAnnouncements_HandlesCancellationToken()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            //Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => _repository.GetAnnouncements(cts.Token));
        }

        /// <summary>
        /// Проверяет, что метод возвращает данные в виде объектов типа AnnouncementItemResponse.
        /// </summary>
        [Fact]
        public async Task GetAnnouncements_MapsCorrectlyToResponse()
        {
            //Arrange & Act
            var result = await _repository.GetAnnouncements(CancellationToken.None);

            //Assert
            Assert.All(result, r => Assert.IsType<AnnouncementItemResponse>(r));
        }



        /// <summary>
        /// Проверяет, что метод возвращает не более 10 элементов.
        /// </summary>
        [Fact]
        public async Task GetAnnouncements_Returns_Maximum10Items()
        {
            _dbContext.AddRange(Enumerable.Range(4, 20).Select(i => new AnnouncementItem
            {
                AnnId = i,
                AnnTitle = $"Title{i}",
                AnnDesc = $"Desc{i}",
                AnnUrl = $"Url{i}",
                Language=$"language{i}",
                CTime = 1000 + i
            }));

            await _dbContext.SaveChangesAsync();

            var result = await _repository.GetAnnouncements(CancellationToken.None);
            Assert.Equal(10, result.Count());
        }


        /// <summary>
        /// Проверяет, что метод возвращает пустую коллекцию, если в базе данных отсутствуют записи.
        /// </summary>
        [Fact]
        public async Task GetAnnouncements_Returns_Empty_WhenDbSetNull()
        {
           
            var result = await _repository.GetAnnouncements(CancellationToken.None);
            Assert.Empty(result);
        }



        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}
