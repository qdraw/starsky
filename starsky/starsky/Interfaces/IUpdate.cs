using System;
using System.Collections.Generic;
using starsky.Models;
using starsky.ViewModels;

namespace starsky.Interfaces
{
    public interface IUpdate
    {

        //IEnumerable<string> GetAll(string subPath = "");
        List<FileIndexItem> GetAllFiles(string subPath = "");

        //IEnumerable<FileIndexItem> GetFilesInFolder(string subPath = "/");

        IEnumerable<FileIndexItem> DisplayFileFolders(string subPath = "/");

        ObjectItem SingleItem(string singleItemDbPath);



        //string GetItemByHash(string path);

        IEnumerable<FileIndexItem> SearchObjectItem(string tag = "", int pageNumber = 0);
        int SearchLastPageNumber(string tag);

        string GetItemByHash(string fileHash);


        FileIndexItem AddItem(FileIndexItem updateStatusContent);

        //IEnumerable<string> AddList(IEnumerable<string> updateStatusContent);

        IEnumerable<string> SyncFiles(string subPath = "/");

        


        //UpdateStatus Update(UpdateStatus updateStatusContent);
        //  ChannelEvent AddOrUpdate(InputChannelEvent updateStatusContent);


        //IEnumerable<UpdateStatus> GetLastMinute(string name);
        //SmileyViewModel CountSmileys();

        //IEnumerable<SqlBotDataEntities> GetAll();
        //string Get(int id);

        //SuccesRatioViewModel GetSuccesRatio();

        //IEnumerable<int> GetMonthlyUsersFinished();
        //IEnumerable<int> GetMonthlyUsersConfirmed();

        //IEnumerable<int> GetHourlyUsers();

        //HappinessStats AddOrUpdateHappinessStats(HappinessStats input);

        //WatsonStats AddOrUpdateWatsonStats(WatsonStats input);

        //WatsonStatsViewModel GetWatsonStats();


        // IEnumerable<FCT_Stats>

        //UpdateStatus GetLatestByName(string name);
        //IEnumerable<UpdateStatus> GetAll();

        //IEnumerable<UpdateStatus> GetAllByName(string name);
        //IEnumerable<UpdateStatus> GetRecentByName(string name);
        /*IEnumerable<ChannelEvent> GetTimeSpanByName(string name, DateTime startDateTime, DateTime endDateTime);

        ChannelUser GetChannelUserIdByUrlSafeName(string nameUrlSafe, bool internalRequest);
        IEnumerable<ChannelUser> GetAllChannelUsers();

        GetStatus IsFree(string channelUserId);
        //EventsOfficeHoursModel Events(DateTime startDateTime, DateTime endDateTime, string urlSafeName);
        EventsOfficeHoursModel EventsDayView(DateTime day, string urlSafeName);
        EventsOfficeHoursModel EventsRecent(string urlSafeName);

        EventsOfficeHoursModel ParseEvents(List<ChannelEvent> channelEvents, DateTime startDateTime,
            DateTime endDateTime);*/


    }
}
