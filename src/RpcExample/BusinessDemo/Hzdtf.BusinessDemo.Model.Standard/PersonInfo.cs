using Hzdtf.Utility.Standard.Model;
using MessagePack;
using Newtonsoft.Json;
using System;

namespace Hzdtf.BusinessDemo.Model.Standard
{
    /// <summary>
    /// 人信息
    /// @ 黄振东
    /// </summary>
    [MessagePackObject]
    public class PersonInfo : SimpleInfo
    {
        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty("name")]
        [Key("name")]
        public string Name
        {
            get;
            set;
        }
    }
}
