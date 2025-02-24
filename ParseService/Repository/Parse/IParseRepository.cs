﻿using ParseService.Models;
using ParseService.Models.Response;

namespace ParseService.Repository.Parse;

public interface IParseRepository
{
    public Task<IEnumerable<AnnouncementItemResponse>> GetAnnouncements(CancellationToken cancellationToken);
    public Task AddAnnouncements(AnnouncementItem announcementItem, CancellationToken cancellationToken);
}
