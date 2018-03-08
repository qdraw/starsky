using System;
using System.Collections.Generic;
using starsky.Models;

namespace starsky.Interfaces
{
    public interface IUpdate
    {

        IEnumerable<string> GetAll();

        FileIndexItem Add(FileIndexItem updateStatusContent);

        IEnumerable<string> AddOrUpdateList(IEnumerable<string> updateStatusContent);

        IEnumerable<string> SyncFiles();

        


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
