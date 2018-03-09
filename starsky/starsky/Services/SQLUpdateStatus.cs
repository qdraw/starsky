using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using starsky.Interfaces;
using starsky.Models;
using Microsoft.Rest.Azure;
using starsky.Data;


namespace starsky.Services
{
    public class SqlUpdateStatus : IUpdate
    {
        private readonly ApplicationDbContext _context;

        public SqlUpdateStatus(ApplicationDbContext context)
        {
            _context = context;
        }

        public string Get(int id)
        {
            throw new NotImplementedException();
        }

        public List<FileIndexItem> GetAll()
        {
            return _context.FileIndex.OrderBy(r => r.FileName).ToList();
        }

        //public IEnumerable<string> GetAll()
        //{
        //    var dbItems = _context.FileIndex.OrderBy(r => r.FileName);
        //    return dbItems.Select(item => item.FilePath).ToList();
        //}

        public IEnumerable<string> RemoveOldFilesByFileList(IEnumerable<string> shortFileList)
        {
            var newFileList = new List<string>();
            foreach (var item in shortFileList)
            {
                if (Files.PathToFull(item) == null)
                {
                    var firstOrDefault = _context.FileIndex.FirstOrDefault(r => r.FilePath == item);
                    if (firstOrDefault != null)
                    {
                        _context.Remove(firstOrDefault);
                    }
                }
                else
                {
                    newFileList.Add(item);
                }
            }

            return newFileList;
        }


     


        public IEnumerable<string> SyncFiles()
        {
            var localFileList = Files.GetFiles().ToList();
            var databaseFileList = GetAll();

            
            // Check for updated files based on hash
            var localFileListFileHash = localFileList.OrderBy(r => r.FileHash).Select(item => item.FileHash).ToList();
            var databaseFileListFileHash = databaseFileList.OrderBy(r => r.FileHash).Select(item => item.FileHash).ToList();

            IEnumerable<string> differenceFileHash = databaseFileListFileHash.Except(localFileListFileHash);

            foreach (var item in differenceFileHash)
            {
                var ditem = databaseFileList.FirstOrDefault(p => p.FileHash == item);
                databaseFileList.Remove(ditem);
                RemoveItem(ditem);
            }



            localFileList.ForEach(item =>
            {
                var dbMatchFirst = _context.FileIndex
                    .FirstOrDefault(p => p.FilePath == Files.PathToUnixStyle(item.FilePath)
                                         && p.FileHash == item.FileHash);

                if (dbMatchFirst == null)
                {
                    item.AddToDatabase = DateTime.Now;
                    item.FilePath = Files.PathToUnixStyle(item.FilePath);
                    AddItem(item);
                    databaseFileList.Add(item);
                }

            });

            //Check fileName Difference
            var localFileListFileName = localFileList.OrderBy(r => r.FileName).Select(item => Files.PathToUnixStyle(item.FilePath)).ToList();
            var databaseFileListFileName = databaseFileList.OrderBy(r => r.FileName).Select(item => item.FilePath).ToList();

            IEnumerable<string> differenceFileNames = databaseFileListFileName.Except(localFileListFileName);

            foreach (var item in differenceFileNames)
            {
                var ditem = databaseFileList.FirstOrDefault(p => p.FilePath == item);
                databaseFileList.Remove(ditem);
                RemoveItem(ditem);
            }



            return null;
        }

        public FileIndexItem AddItem(FileIndexItem updateStatusContent)
        {
            _context.FileIndex.Add(updateStatusContent);
            _context.SaveChanges();
            return updateStatusContent;
        }

        public FileIndexItem RemoveItem(FileIndexItem updateStatusContent)
        {
            _context.FileIndex.Remove(updateStatusContent);
            _context.SaveChanges();
            return updateStatusContent;
        }



        //public bool IfInDatabase(string filePath)
        //{
        //    var count = _context.FileIndex.Count(r => r.FilePath == filePath);
        //    switch (count)
        //    {
        //        case 0:
        //            return false;
        //        default:
        //            return true;
        //    }
        //}






        //public IEnumerable<string> AddList(IEnumerable<string> inputStats)
        //{
        //    var addOrUpdateList = inputStats.ToList();
        //    foreach (var item in addOrUpdateList)
        //    {
        //        if (IfInDatabase(item))
        //        {

        //            //var item = GetWatsonStatsUser(inputStats.UserId);
        //            //item.UserId = inputStats.UserId;
        //            //item.LastActivity = inputStats.LastActivity;
        //            //item.RequestTemperatureSettings = inputStats.RequestTemperatureSettings;
        //            //_context.Attach(item).State = EntityState.Modified;
        //            //_context.SaveChanges();

        //        }
        //        else
        //        {
        //            var newItem = new FileIndexItem();
        //            newItem.FileName = Path.GetFileName(item);
        //            newItem.FilePath = Files.PathToUnixStyle(item);

        //            _context.FileIndex.Add(newItem);
        //            _context.SaveChanges();
        //        }
        //    }

        //    return inputStats;
        //}

        //public IEnumerable<SqlBotDataEntities> GetAll()
        //{
        //    var t = _context.SqlBotDataEntities.OrderBy(r => r.Id);
        //    return t;
        //}

        //public SmileyViewModel CountSmileys()
        //{
        //    var smiley = new SmileyViewModel();

        //    try
        //    {
        //        smiley.Happy = _context.HappinessStats.Count(p => p.Smiley == Smiley.Happy);
        //        smiley.Neutral = _context.HappinessStats.Count(p => p.Smiley == Smiley.Neutral);
        //        smiley.Unhappy = _context.HappinessStats.Count(p => p.Smiley == Smiley.Sad);
        //    }
        //    catch (SqlException)
        //    {
        //        smiley.Happy = 1;
        //        smiley.Neutral = 1;
        //        smiley.Unhappy = 1;
        //    }

        //    return smiley;
        //}

        //public SuccesRatioViewModel GetSuccesRatio() {
        //    var value = new SuccesRatioViewModel();

        //    var recentTime = DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 10, 0));

        //    value.TotalActiveUsers = _context.FCT_Stats.Distinct().Count(p => p.EindDatumtijd > recentTime);
        //    value.TotalConfirmed = _context.FCT_Stats.Count(p => p.AdresBevestigd);
        //    value.TotalFinished = _context.FCT_Stats.Count(p => p.Afgerond && p.AdresBevestigd);
        //    value.TotalUsers = _context.FCT_Stats.Count();
        //    return value;
        //}

        //public IEnumerable<int> GetHourlyUsers()
        //{

        //    var value = new List<int>();
        //    var startDateTime = DateTime.UtcNow.Subtract(new TimeSpan(0, 24, 0, 0));
        //    var content = _context.FCT_Stats.Where(p => p.EindDatumtijd >= startDateTime).ToList();

        //    const int interval = 3600 * 2; // 2 hour
        //    var i = DateTimeDtos.GetUnixTime(startDateTime);
        //    while (i <= DateTimeDtos.GetUnixTime(DateTime.Now))
        //    {
        //        var start = DateTimeDtos.UnixTimeToDateTime(i);
        //        var end = DateTimeDtos.UnixTimeToDateTime(i + interval);
        //        var item = content.Distinct().Count(p => p.EindDatumtijd > start && p.EindDatumtijd < end);
        //        value.Add(item);
        //        i += interval;
        //    }

        //    return value;
        //}

        //public IEnumerable<int> GetMonthlyUsersFinished()
        //{
        //    var value = new List<int>();
        //    var startDateTime = DateTime.UtcNow.Subtract(new TimeSpan(31, 0, 0, 0));
        //    var content = _context.FCT_Stats.Where(p => p.StartDatumtijd >= startDateTime && p.Afgerond).ToList();

        //    const int interval = 86400; // 1 day
        //    var i = DateTimeDtos.GetUnixTime(startDateTime);
        //    while (i <= DateTimeDtos.GetUnixTime(DateTime.Now))
        //    {
        //        var start = DateTimeDtos.UnixTimeToDateTime(i);
        //        var end = DateTimeDtos.UnixTimeToDateTime(i + interval);
        //        var item = content.Distinct().Count(p => p.StartDatumtijd > start && p.StartDatumtijd < end);
        //        value.Add(item);
        //        i += interval;
        //    }

        //    return value;
        //}
        //public IEnumerable<int> GetMonthlyUsersConfirmed()
        //{
        //    var value = new List<int>();
        //    var startDateTime = DateTime.UtcNow.Subtract(new TimeSpan(31, 0, 0, 0));
        //    var content = _context.FCT_Stats.Where(p => p.StartDatumtijd >= startDateTime && p.AdresBevestigd).ToList();

        //    const int interval = 86400; // 1 day
        //    var i = DateTimeDtos.GetUnixTime(startDateTime);
        //    while (i <= DateTimeDtos.GetUnixTime(DateTime.Now))
        //    {
        //        var start = DateTimeDtos.UnixTimeToDateTime(i);
        //        var end = DateTimeDtos.UnixTimeToDateTime(i + interval);
        //        var item = content.Distinct().Count(p => p.StartDatumtijd > start && p.StartDatumtijd < end);
        //        value.Add(item);
        //        i += interval;
        //    }

        //    return value;
        //}



        //public HappinessStats GetHappinessStatsUser(string gebruikersID)
        //{
        //    var userIdObjectIsAccessible = _context.HappinessStats.LastOrDefault(p => p.GebruikersID == gebruikersID);
        //    return userIdObjectIsAccessible;
        //}

        //public HappinessStats AddOrUpdateHappinessStats(HappinessStats inputStats)
        //{

        //    if (IsUserHappinessStatsInDatabase(inputStats.GebruikersID))
        //    {

        //        var item = GetHappinessStatsUser(inputStats.GebruikersID);

        //        item.GebruikersID = inputStats.GebruikersID;
        //        item.DateTime = inputStats.DateTime;
        //        item.Smiley = inputStats.Smiley;

        //        _context.Attach(item).State = EntityState.Modified;
        //        _context.SaveChanges();

        //        return item;

        //    }
        //    else
        //    {
        //        _context.HappinessStats.Add(inputStats);
        //        _context.SaveChanges();
        //        return inputStats;
        //    }
        //}


        //public bool IsUserWatsonStatsInDatabase(string userid)
        //{
        //    var count = _context.WatsonStats.Count(r => r.UserId == userid);
        //    switch (count)
        //    {
        //        case 0:
        //            return false;
        //        default:
        //            return true;
        //    }
        //}

        //public WatsonStats GetWatsonStatsUser(string userid)
        //{
        //    var userIdObjectIsAccessible = _context.WatsonStats.LastOrDefault(p => p.UserId == userid);
        //    return userIdObjectIsAccessible;
        //}




        //public WatsonStatsViewModel GetWatsonStats() { 
        //    var model = new WatsonStatsViewModel();
        //    model.OnlyLoggedIn = _context.WatsonStats.Count(p => p.LoggedIn && !p.RequestTemperatureSettings);
        //    model.RequestTemperatureSetting = _context.WatsonStats.Count(p => p.RequestTemperatureSettings && p.LoggedIn);
        //    model.Nologin = _context.WatsonStats.Count(p => !p.RequestTemperatureSettings && !p.LoggedIn );
        //    return model;
        //}



        /*
public IEnumerable<ChannelEvent> GetTimeSpanByName(string urlsafename, DateTime startDateTime,
   DateTime endDateTime)
{
   var result = _context.ChannelEvent
       .Where(p => p.DateTime >= startDateTime && p.DateTime <= endDateTime)
       .Where(b => b.ChannelUser.NameUrlSafe == urlsafename);
   return result.AsEnumerable();
}


public ChannelUser Add(ChannelUser updateStatusContent)
{
   _context.ChannelUser.Add(updateStatusContent);
   _context.SaveChanges();
   return updateStatusContent;
}

// used for unit tests
public ChannelUser Update(ChannelUser updateUser)
{
   _context.Attach(updateUser).State = EntityState.Modified;
   _context.SaveChanges();
   return updateUser;
}

// used for unit tests
public ChannelEvent Update(ChannelEvent updateEvent)
{
   _context.Attach(updateEvent).State = EntityState.Modified;
   _context.SaveChanges();
   return updateEvent;
}




public bool IsUserInDatabase(string nameUrlSafe)
{
   var count = _context.ChannelUser.Count(r => r.NameUrlSafe == nameUrlSafe);
   switch (count)
   {
       case 0:
           return false;
       default:
           return true;
   }
}


public ChannelUser GetChannelUserIdByUrlSafeName(string nameUrlSafe, bool internalRequest)
{
   var userIdObjectIsAccessible = _context.ChannelUser.LastOrDefault(p => p.NameUrlSafe == nameUrlSafe);

   if (userIdObjectIsAccessible == null) return null;

   if (!internalRequest)
   {
       if (userIdObjectIsAccessible.IsAccessible)
       {
           return userIdObjectIsAccessible; // return null if not there
       }
       else
       {
           return null;
       }
   }

   return userIdObjectIsAccessible;
}


public IEnumerable<ChannelUser> GetAllChannelUsers()
{
   return _context.ChannelUser; // return null if not there
}

public ChannelUser AddUser(string name)
{
   if (string.IsNullOrWhiteSpace(name)) return null;

   var nameUrlSafe = name.ToLower();
   nameUrlSafe = Regex.Replace(nameUrlSafe, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
   var newChannelUser = new ChannelUser
   {
       Name = name,
       IsVisible = true,
       IsAccessible = true,
       NameUrlSafe = nameUrlSafe
   };

   _context.ChannelUser.Add(newChannelUser);
   _context.SaveChanges();

   return newChannelUser;
}

public IEnumerable<ChannelEvent> GetLastMinute(string channelUserId)
{
   // round minute to 18:48 for example
   var now = DateTime.UtcNow;
   var nowTicks = now.Ticks;

   var lastMinute = new DateTime(nowTicks - (nowTicks % (1000 * 1000 * 10 * 60))); // 60

   var lastMinuteRequests = _context.ChannelEvent
       .Where(p => p.DateTime > lastMinute)
       .Where(b => b.ChannelUserId == channelUserId);

   return lastMinuteRequests;
}


public GetStatus IsFree(string channelUserId)
{
   var latestEvent = _context.ChannelEvent
       .LastOrDefault(b => b.ChannelUserId == channelUserId);

   if (latestEvent == null)
   {
       return new GetStatus
       {
           IsFree = true
       };
   }

   var difference = DateTime.UtcNow - latestEvent.DateTime;

   var isFreeStatus = new GetStatus
   {
       DateTime = DateTime.SpecifyKind(latestEvent.DateTime, DateTimeKind.Utc),
       Difference = difference
   };

   var differentTimeSpan = (new TimeSpan(0, 2, 0)); // 2 minutes
   if (difference >= differentTimeSpan) 
   {
       isFreeStatus.IsFree = true;
   }

   return isFreeStatus;
}

public EventsOfficeHoursModel EventsRecent(string urlSafeName)
{
   var dto = new DateDto();

   var startDateTime =
       dto.RoundDown(
           DateTime.UtcNow.Subtract(new TimeSpan(0, 8, 0, 0)),
           new TimeSpan(0, 0, 5, 0));

   var endDateTime = dto.RoundUp(DateTime.UtcNow, new TimeSpan(0, 0, 5, 0));

   var channelUser = GetChannelUserIdByUrlSafeName(urlSafeName, true);
   if (channelUser == null)
   {
       return null;
   }
   var channelUserId = channelUser.NameId;

   var channelEvents = _context.ChannelEvent
       .Where(
           p => p.ChannelUserId == channelUserId &&
                p.DateTime > startDateTime &&
                p.DateTime < endDateTime
       ).OrderBy(p => p.DateTime);

   return ParseEvents(channelEvents.ToList(), startDateTime, endDateTime);
}

public EventsOfficeHoursModel EventsDayView(DateTime dateTime, string urlSafeName)
{
   var dto = new DateDto();

   var startDateTime = dateTime;
   var endDateTime = dateTime.AddHours(24);

   var channelUser = GetChannelUserIdByUrlSafeName(urlSafeName, true);
   if (channelUser == null)
   {
       return null;
   }

   var channelUserId = channelUser.NameId;

   var channelEvents = _context.ChannelEvent
       .Where(
           p => p.ChannelUserId == channelUserId &&
                p.DateTime > startDateTime &&
                p.DateTime < endDateTime
       ).OrderBy(x => x.DateTime).ToList();

   if (channelEvents.Count == 0)
   {
       var startEmthyDateTime =
           dto.RoundDown(
               DateTime.UtcNow.Subtract(new TimeSpan(0, 8, 0, 0)),
               new TimeSpan(0, 0, 5, 0));

       var endEmthyDateTime = dto.RoundUp(DateTime.UtcNow, new TimeSpan(0, 0, 5, 0));

       var model = new EventsOfficeHoursModel
       {
           Day = startDateTime.DayOfWeek,
           StartDateTime = startEmthyDateTime,
           EndDateTime = endEmthyDateTime,
           AmountOfMotions = new List<WeightViewModel>(),
           Length = 0
       };
       return model;
   }

   var median = channelEvents.Skip(channelEvents.Count() / 2).First().DateTime;
   startDateTime = dto.RoundDown(median.ToUniversalTime().AddHours(-4), new TimeSpan(0, 0, 5, 0));
   endDateTime = dto.RoundUp(median.ToUniversalTime().AddHours(4), new TimeSpan(0, 0, 5, 0));

   var channelParseEvents = channelEvents
       .Where(p => p.DateTime > startDateTime &&
                   p.DateTime < endDateTime);

   return ParseEvents(channelParseEvents.ToList(), startDateTime, endDateTime);

}

public EventsOfficeHoursModel ParseEvents(List<ChannelEvent> channelEvents, DateTime startDateTime, DateTime endDateTime)
{
   var dto = new DateDto();

   var model = new EventsOfficeHoursModel
   {
       Day = startDateTime.DayOfWeek,
       StartDateTime = startDateTime,
       EndDateTime = endDateTime,
       AmountOfMotions = new List<WeightViewModel>(),
       Length = 0
   };

   model.Length = _context.ChannelEvent
       .Count(
           p => p.DateTime > model.StartDateTime &&
                p.DateTime < model.EndDateTime
       );

   const int interval = 60 * 5; // 5 minutes
   var i = dto.GetUnixTime(startDateTime);
   while (i <= dto.GetUnixTime(endDateTime))
   {

       var eventItem = new WeightViewModel
       {
           StartDateTime = dto.UnixTimeToDateTime(i),
           EndDateTime = dto.UnixTimeToDateTime(i + interval)
       };
       eventItem.LabelUtc = eventItem.StartDateTime.ToString("HH:mm");
       eventItem.Label = dto.UtcDateTimeToAmsterdamDateTime(eventItem.StartDateTime).ToString("HH:mm");

       var weightSum = channelEvents
           .Where(p =>
               p.DateTime > eventItem.StartDateTime &&
               p.DateTime < eventItem.EndDateTime)
           .Select(p => p.Weight).Sum();

       eventItem.Weight = weightSum;
       model.AmountOfMotions.Add(eventItem);
       i += interval;
   }
   return model;

}

*/
    }






    //public UpdateStatus Get(int id)
    //{
    //    return _context.UpdateStatus.FirstOrDefault(r => r.Id == id);
    //}

    //public IEnumerable<UpdateStatus> GetAll()
    //{
    //    return _context.UpdateStatus.OrderBy(r => r.Id);
    //}

    //public IEnumerable<UpdateStatus> GetAllByName(string name)
    //{
    //    return _context.UpdateStatus.Where(b => b.Name == name);
    //}



}