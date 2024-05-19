using System;
using System.Collections.Generic;

namespace MarineBot.Entities
{
    public class MALAnimeEntry
    {
        public int? Mal_Id { get; set; }
        public string Url { get; set; }
        public MALImageSet Images { get; set; }
        public bool? Approved { get; set; }
        public List<MALTitle> Titles { get; set; }
        public string Title { get; set; }
        public string Title_English { get; set; }
        public string Title_Japanese { get; set; }
        public List<string> Title_Synonyms { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public int? Episodes { get; set; }
        public string Status { get; set; }
        public bool? Airing { get; set; }
        public MALAiredPeriod Aired { get; set; }
        public string Duration { get; set; }
        public string Rating { get; set; }
        public double? Score { get; set; }
        public int? Scored_By { get; set; }
        public int? Rank { get; set; }
        public int? Popularity { get; set; }
        public int? Members { get; set; }
        public int? Favorites { get; set; }
        public string Synopsis { get; set; }
        public string Background { get; set; }
        public string Season { get; set; }
        public int? Year { get; set; }
        public MALBroadcastTime Broadcast { get; set; }
        public List<MALEntityInfo> Producers { get; set; }
        public List<MALEntityInfo> Licensors { get; set; }
        public List<MALEntityInfo> Studios { get; set; }
        public List<MALEntityInfo> Genres { get; set; }
        public List<MALEntityInfo> Explicit_Genres { get; set; }
        public List<MALEntityInfo> Themes { get; set; }
        public List<MALEntityInfo> Demographics { get; set; }
    }

    public class MALImageSet
    {
        public MALImageUrls Jpg { get; set; }
        public MALImageUrls Webp { get; set; }
    }

    public class MALImageUrls
    {
        public string Image_Url { get; set; }
        public string Small_Image_Url { get; set; }
        public string Large_Image_Url { get; set; }
    }

    public class MALTitle
    {
        public string Type { get; set; }
        public string Title { get; set; }
    }

    public class MALAiredPeriod
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public MALAiredProp Prop { get; set; }
        public string String { get; set; }
    }

    public class MALAiredProp
    {
        public MALDateInfo From { get; set; }
        public MALDateInfo To { get; set; }
    }

    public class MALDateInfo
    {
        public int? Day { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
    }

    public class MALBroadcastTime
    {
        public string Day { get; set; }
        public string Time { get; set; }
        public string Timezone { get; set; }
        public string String { get; set; }
    }

    public class MALEntityInfo
    {
        public int Mal_Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

}