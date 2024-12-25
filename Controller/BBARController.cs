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
using HIS_DB_Lib;
namespace DB2VM
{
    [Route("dbvm/[controller]")]
    [ApiController]
    public class BBARController : ControllerBase
    {
       
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



        static string MySQL_server = $"{ConfigurationManager.AppSettings["MySQL_server"]}";
        static string MySQL_database = $"{ConfigurationManager.AppSettings["MySQL_database"]}";
        static string MySQL_userid = $"{ConfigurationManager.AppSettings["MySQL_user"]}";
        static string MySQL_password = $"{ConfigurationManager.AppSettings["MySQL_password"]}";
        static string MySQL_port = $"{ConfigurationManager.AppSettings["MySQL_port"]}";

        private SQLControl sQLControl_UDSDBBCM = new SQLControl(MySQL_server, "dps01", "medicine_page", MySQL_userid, MySQL_password, (uint)MySQL_port.StringToInt32(), MySql.Data.MySqlClient.MySqlSslMode.None);
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
            orderClass.藥袋條碼 = $"{BarCode}";
            orderClass.藥品碼 = 藥品碼;
            orderClass.藥品名稱 = list_藥品資料[0][(int)enum_藥品資料_藥檔資料.藥品名稱].ObjectToString();
            orderClass.病人姓名 = "";
            orderClass.病歷號 = "";
            orderClass.頻次 = "";
            orderClass.途徑 = "";
            orderClass.交易量 = num.ToString();
            orderClass.開方日期 = DateTime.Now.ToDateTimeString();
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
                    value[(int)enum_醫囑資料.開方日期] = orderClasses[i].開方日期;
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


        [Route("full/{BarCode}")]
        [HttpGet]
        public string Getfull(string BarCode)
        {
            returnData returnData = new returnData();
            MyTimerBasic myTimerBasic = new MyTimerBasic(50000);
            if (BarCode.StringIsEmpty())
            {
                returnData.Code = -200;
                returnData.Result = $"BarCode value failed! {BarCode}";
                return returnData.JsonSerializationt(true);
            }
            if (BarCode.Length < 5)
            {
                returnData.Code = -200;
                returnData.Result = $"BarCode value failed! {BarCode}";
                return returnData.JsonSerializationt(true);
            }
            List<object[]> list_藥品資料 = this.sQLControl_UDSDBBCM.GetAllRows(null);

            string[] BarCode_Ary = BarCode.Split(';');
            string 藥品碼 = "";
            string 床號 = "";
            string 日期_temp = "";
            string 病人姓名 = "";
            string 病歷號 = "";
            string 領藥號 = "";
            string 交易量 = "";
            bool flag_OK = false;
            if (BarCode_Ary.Length == 5)
            {
                藥品碼 = BarCode_Ary[0].Substring(BarCode_Ary[0].Length - 5, 5);
                床號 = BarCode_Ary[0].Substring(11, 5);
                日期_temp = BarCode_Ary[1];
                病人姓名 = BarCode_Ary[2];
                病歷號 = BarCode_Ary[3];
                交易量 = BarCode_Ary[4];

                if (交易量.StringIsInt32() == false)
                {
                    returnData.Code = -200;
                    returnData.Result = $"交易量 error!{交易量}";
                    return returnData.JsonSerializationt(true);
                }
                交易量 = (交易量.StringToInt32() * -1).ToString();
                if (日期_temp.Length != 7)
                {
                    returnData.Code = -200;
                    returnData.Result = $"Date error!{日期_temp}";
                    return returnData.JsonSerializationt(true);
                }
                else
                {
                    string Year = 日期_temp.Substring(0, 3);
                    string Month = 日期_temp.Substring(3, 2);
                    string Day = 日期_temp.Substring(5, 2);
                    Year = (Year.StringToInt32() + 1911).ToString();
                    日期_temp = $"{Year}/{Month}/{Day}";
                }

                flag_OK = true;         
            }
            if(flag_OK == false)
            {
                BarCode_Ary = BarCode.Split('_');
                if(BarCode_Ary.Length == 6)
                {
                    藥品碼 = BarCode_Ary[3].Substring(BarCode_Ary[3].Length - 5, 5);
                    病人姓名 = BarCode_Ary[4];
                    日期_temp = BarCode_Ary[0];
                    病歷號 = BarCode_Ary[1];
                    領藥號 = BarCode_Ary[2];
                    交易量 = BarCode_Ary[5];
                    if (交易量.StringIsInt32() == false)
                    {
                        returnData.Code = -200;
                        returnData.Result = $"交易量 error!{交易量}";
                        return returnData.JsonSerializationt(true);
                    }
                    交易量 = (交易量.StringToInt32() * -1).ToString();
                    if (日期_temp.Length <= 8)
                    {
                        returnData.Code = -200;
                        returnData.Result = $"Date error!{日期_temp}";
                        return returnData.JsonSerializationt(true);
                    }
                    else
                    {
                        string Year = 日期_temp.Substring(0, 4);
                        string Month = 日期_temp.Substring(4, 2);
                        string Day = 日期_temp.Substring(6, 2);
                        日期_temp = $"{Year}/{Month}/{Day}";
                    }
                    flag_OK = true;
                }
            }
            if(flag_OK == false)
            {
                returnData.Code = -200;
                returnData.Result = $"BarCode error!  {BarCode}";
                return returnData.JsonSerializationt(true);
            }

            list_藥品資料 = list_藥品資料.GetRows((int)enum_藥品資料_藥檔資料.藥品碼, 藥品碼);
            if (list_藥品資料.Count == 0)
            {
                returnData.Code = -200;
                returnData.Result = $"找不到此藥品 {藥品碼}";
                return returnData.JsonSerializationt(true);
            }

            List<object[]> list_value_Add = new List<object[]>();
            List<OrderClass> orderClasses = new List<OrderClass>();
            OrderClass orderClass = new OrderClass();
            orderClass.PRI_KEY = $"{BarCode}";
            orderClass.藥局代碼 = "OPD";
            orderClass.藥袋條碼 = $"{BarCode}";
            orderClass.藥品碼 = 藥品碼;
            orderClass.藥品名稱 = list_藥品資料[0][(int)enum_藥品資料_藥檔資料.藥品名稱].ObjectToString();
            orderClass.病人姓名 = 病人姓名;
            orderClass.病歷號 = 病歷號;
            orderClass.劑量單位 = list_藥品資料[0][(int)enum_藥品資料_藥檔資料.包裝單位].ObjectToString();
            orderClass.床號 = 床號;
            orderClass.頻次 = "";
            orderClass.途徑 = "";
            orderClass.交易量 = 交易量;
            orderClass.領藥號 = 領藥號;
            orderClass.開方日期 = 日期_temp;
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
                    value[(int)enum_醫囑資料.領藥號] = orderClasses[i].領藥號;
                    value[(int)enum_醫囑資料.藥品碼] = orderClasses[i].藥品碼;
                    value[(int)enum_醫囑資料.藥品名稱] = orderClasses[i].藥品名稱;
                    value[(int)enum_醫囑資料.病歷號] = orderClasses[i].病歷號;
                    value[(int)enum_醫囑資料.藥袋條碼] = orderClasses[i].藥袋條碼;
                    value[(int)enum_醫囑資料.病人姓名] = orderClasses[i].病人姓名;
                    value[(int)enum_醫囑資料.交易量] = orderClasses[i].交易量;
                    value[(int)enum_醫囑資料.床號] = orderClasses[i].床號;
                    value[(int)enum_醫囑資料.開方日期] = orderClasses[i].開方日期;
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
            returnData.Code = 200;
            returnData.TimeTaken = myTimerBasic.ToString();
            returnData.Result = $"取得醫令成功,共新增<{list_value_Add.Count}>筆資料";
            returnData.Data = orderClasses;
            return returnData.JsonSerializationt();


        }
    }
}
