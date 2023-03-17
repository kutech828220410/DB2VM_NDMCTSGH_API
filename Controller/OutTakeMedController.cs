using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IBM.Data.DB2.Core;
using System.Data;
using System.Configuration;
using Basic;
using Oracle.ManagedDataAccess.Client;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace DB2VM_API
{
    public class class_OutTakeMed_data
    {
        [JsonPropertyName("MC_name")]
        public string 電腦名稱 { get; set; }
        [JsonPropertyName("code")]
        public string 藥品碼 { get; set; }
        [JsonPropertyName("value")]
        public string 交易量 { get; set; }
        [JsonPropertyName("operator")]
        public string 操作人 { get; set; }
        [JsonPropertyName("ID")]
        public string ID { get; set; }
        [JsonPropertyName("patient_name")]
        public string 病人姓名 { get; set; }
        [JsonPropertyName("patient_code")]
        public string 病歷號 { get; set; }
        [JsonPropertyName("prescription_time")]
        public string 開方時間 { get; set; }
        [JsonPropertyName("OP_type")]
        public string 功能類型 { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class OutTakeMedController
    {
        [Route("Sample")]
        [HttpGet()]
        public string Get_Sample()
        {
            string str = Basic.Net.WEBApiGet(@"http://10.14.16.50:443/api/OutTakeMed/Sample");

            return str;
        }
        [HttpPost]
        public string Post([FromBody] List<class_OutTakeMed_data> data)
        {
            string json = data.JsonSerializationt();
            string str = Basic.Net.WEBApiPostJson("http://10.14.16.50:443/api/OutTakeMed/", json);
            return str;
        }
    }
}
