namespace ParseService.Models.Response
{
    public record AnnouncementItemResponse(int AnnId ,
                                            string AnnTitle,
                                            string AnnDesc,
                                            string AnnUrl
    );
  
}
