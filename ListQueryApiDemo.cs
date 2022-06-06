using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class ListQueryApiDemo
    {
        public static void listQuery()
        {     
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 易盾反垃圾云服务名单提交接口地址  */
            String apiUrl = "http://as.dun.163.com/v2/list/pageQuery";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("businessId", businessId);
            parameters.Add("version", "v2");
            parameters.Add("timestamp", time);
            parameters.Add("nonce", new Random().Next().ToString());

            // 2.设置私有参数
            // 1: 白名单，2: 黑名单，4: 必审名单，8: 预审名单
            parameters.Add("listType", "2");
            // 1: 用户名单，2: IP名单
            parameters.Add("entityType", "1");
            parameters.Add("entity", "用户黑名单1");
            parameters.Add("startTime", "1654142142776");
            parameters.Add("endTime", "1654142142886");
            parameters.Add("pageNum", "1");
            parameters.Add("pageSize", "10");

            // 3.生成签名信息
            String signature = Utils.genSignature(secretKey, parameters);
            parameters.Add("signature", signature);

            // 4.发送HTTP请求
            HttpClient client = Utils.makeHttpClient();
            String result = Utils.doPost(client, apiUrl, parameters, 1000);
            if(result != null)
            {
                JObject ret = JObject.Parse(result);
                int code = ret.GetValue("code").ToObject<Int32>();
                String msg = ret.GetValue("msg").ToObject<String>();
                if (code == 200)
                {
                    JObject re = ret.GetValue("result").ToObject<JObject>();
                    Console.WriteLine(String.Format("SECCESS: code={0}, msg={1}, result={2}", code, msg, re));
                }
                else
                {
                    Console.WriteLine(String.Format("ERROR: code={0}, msg={1}", code, msg));
                }
            }
            else
            {
                Console.WriteLine("Request failed!");
            }

        }      
    }
}
