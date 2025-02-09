using Microsoft.EntityFrameworkCore;
using ParseService.Data;
using ParseService.Models;
using ParseService.Models.Response;

namespace ParseService.Repository
{
    public class ParseRepository : IParseRepository
    {
        private readonly ParseDbContext _parseDbContext;

        public ParseRepository(ParseDbContext parseDbContext)
        {
            _parseDbContext = parseDbContext;
        }
        /// <summary>
        /// Добавляет новое объявление в базу данных.
        /// </summary>
        public async Task AddAnnouncements(AnnouncementItem announcementItem,
                                           CancellationToken cancellationToken)
        {
            await _parseDbContext.Announcements.AddAsync(announcementItem,cancellationToken);
            await _parseDbContext.SaveChangesAsync(cancellationToken);
        }
        /// <summary>
        /// Получает последние 10 объявлений, отсортированных по убыванию времени создания.
        /// </summary>
        public async Task<IEnumerable<AnnouncementItemResponse>> GetAnnouncements(CancellationToken cancellationToken)
        {
                  var announcements =  await _parseDbContext.Announcements
                            .AsNoTracking()
                            .OrderByDescending(a => a.CTime)
                            .Take(10)
                            .Select(a => new AnnouncementItemResponse(a.AnnId,a.AnnTitle,a.AnnDesc,a.AnnUrl))
                            .ToListAsync(cancellationToken);

            return announcements is null ? Enumerable.Empty<AnnouncementItemResponse>() : announcements;
        }
    }
}
