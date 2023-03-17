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
using SQLUI;
using Oracle.ManagedDataAccess.Client;
namespace DB2VM
{
    [Route("dbvm/[controller]")]
    [ApiController]
    public class BBARController : ControllerBase
    {
        public enum enum_醫囑資料
        {
            GUID,
            PRI_KEY,
            藥局代碼,
            藥袋條碼,
            藥品碼,
            藥品名稱,
            病人姓名,
            病歷號,
            交易量,
            開方日期,
            產出時間,
            過帳時間,
            狀態,
        }
        public enum enum_急診藥袋
        {
            本次領藥號,
            看診日期,
            病歷號,
            序號,
            頻率,
            途徑,
            總量,
            前次領藥號,
            本次醫令序號,
        }
        public enum enum_藥品資料_藥檔資料
        {
            GUID,
            藥品碼,
            藥品中文名稱,
            藥品名稱,
            藥品學名,
            藥品群組,
            健保碼,
            藥品條碼,
            包裝單位,
            庫存,
            安全庫存,
            圖片網址,
            警訊藥品,
        }




        static string MySQL_server = $"{ConfigurationManager.AppSettings["MySQL_server"]}";
        static string MySQL_database = $"{ConfigurationManager.AppSettings["MySQL_DPS01_database"]}";
        static string MySQL_userid = $"{ConfigurationManager.AppSettings["MySQL_user"]}";
        static string MySQL_password = $"{ConfigurationManager.AppSettings["MySQL_password"]}";
        static string MySQL_port = $"{ConfigurationManager.AppSettings["MySQL_port"]}";

        private SQLControl sQLControl_UDSDBBCM = new SQLControl(MySQL_server, MySQL_database, "medicine_page", MySQL_userid, MySQL_password, (uint)MySQL_port.StringToInt32(), MySql.Data.MySqlClient.MySqlSslMode.None);
        private SQLControl sQLControl_醫囑資料 = new SQLControl(MySQL_server, MySQL_database, "order_list", MySQL_userid, MySQL_password, (uint)MySQL_port.StringToInt32(), MySql.Data.MySqlClient.MySqlSslMode.None);

        [HttpGet]
        public string Get(string BarCode , string num)
        {
            if (num.StringIsInt32() == false) return $"num value failed! {num}";
            if(BarCode.StringIsEmpty()) return $"BarCode value failed! {BarCode}";
            if(BarCode.Length < 5) return $"BarCode value failed! {BarCode}";
            List<object[]> list_藥品資料 = this.sQLControl_UDSDBBCM.GetAllRows(null);

            string 藥品碼 = BarCode.Substring(BarCode.Length - 5, 5);

            list_藥品資料 = list_藥品資料.GetRows((int)enum_藥品資料_藥檔資料.藥品碼, 藥品碼);
            if (list_藥品資料.Count == 0) return $"找不到此藥品 {藥品碼}";

            List<object[]> list_value_Add = new List<object[]>();
            List<OrderClass> orderClasses = new List<OrderClass>();
            OrderClass orderClass = new OrderClass();
            orderClass.PRI_KEY = Guid.NewGuid().ToString();
            orderClass.藥局代碼 = "OPD";
            orderClass.處方序號 = "";
            orderClass.藥袋條碼 = $"{BarCode}";
            orderClass.藥品碼 = 藥品碼;
            orderClass.藥品名稱 = list_藥品資料[0][(int)enum_藥品資料_藥檔資料.藥品名稱].ObjectToString();
            orderClass.病人姓名 = "";
            orderClass.病歷號 = "";
            orderClass.包裝單位 = list_藥品資料[0][(int)enum_藥品資料_藥檔資料.包裝單位].ObjectToString();
            orderClass.劑量 = "";
            orderClass.頻次 = "";
            orderClass.途徑 = "";
            orderClass.天數 = "";
            orderClass.交易量 = num.ToString();
            orderClass.開方時間 = DateTime.Now.ToDateTimeString();
            orderClasses.Add(orderClass);

            for (int i = 0; i < orderClasses.Count; i++)
            {
                List<object[]> list_value = this.sQLControl_醫囑資料.GetRowsByDefult(null, enum_醫囑資料.PRI_KEY.GetEnumName(), orderClasses[i].PRI_KEY);
                if (list_value.Count == 0)
                {
                    object[] value = new object[new enum_醫囑資料().GetLength()];
                    value[(int)enum_醫囑資料.GUID] = Guid.NewGuid().ToString();
                    value[(int)enum_醫囑資料.PRI_KEY] = orderClasses[i].PRI_KEY;
                    value[(int)enum_醫囑資料.藥局代碼] = orderClasses[i].藥局代碼;
                    value[(int)enum_醫囑資料.藥品碼] = orderClasses[i].藥品碼;
                    value[(int)enum_醫囑資料.藥品名稱] = orderClasses[i].藥品名稱;
                    value[(int)enum_醫囑資料.病歷號] = orderClasses[i].病歷號;
                    value[(int)enum_醫囑資料.藥袋條碼] = orderClasses[i].藥袋條碼;
                    value[(int)enum_醫囑資料.病人姓名] = orderClasses[i].病人姓名;
                    value[(int)enum_醫囑資料.交易量] = orderClasses[i].交易量;
                    value[(int)enum_醫囑資料.開方日期] = orderClasses[i].開方時間;
                    value[(int)enum_醫囑資料.產出時間] = DateTime.Now.ToDateTimeString_6();
                    value[(int)enum_醫囑資料.過帳時間] = DateTime.MinValue.ToDateTimeString_6();
                    value[(int)enum_醫囑資料.狀態] = "未過帳";
                    list_value_Add.Add(value);
                }
            }

            if (list_value_Add.Count > 0)
            {
                this.sQLControl_醫囑資料.AddRows(null, list_value_Add);
            }
            return orderClasses.JsonSerializationt();


        }
    }
}
