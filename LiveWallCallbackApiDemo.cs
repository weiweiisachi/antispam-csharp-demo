﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Com.Netease.Is.Antispam.Demo
{
    class LiveWallCallbackApiDemo
    {
        public static void liveWallCallBack()
        {
            /** 产品密钥ID，产品标识 */
            String secretId = "your_secret_id";
            /** 产品私有密钥，服务端生成签名信息使用，请严格保管，避免泄露 */
            String secretKey = "your_secret_key";
            /** 业务ID，易盾根据产品业务特点分配 */
            String businessId = "your_business_id";
            /** 易盾反垃圾云服务直播电视墙离线结果获取接口地址 */
            String apiUrl = "http://as.dun.163.com/v3/livewall/callback/results";
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            long curr = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            String time = curr.ToString();

            // 1.设置公共参数
            parameters.Add("secretId", secretId);
            parameters.Add("businessId", businessId);
            parameters.Add("version", "v3");
            parameters.Add("timestamp", time);
            parameters.Add("nonce", new Random().Next().ToString());

            // 2.生成签名信息
            String signature = Utils.genSignature(secretKey, parameters);
            parameters.Add("signature", signature);

            // 3.发送HTTP请求
            HttpClient client = Utils.makeHttpClient();
            String result = Utils.doPost(client, apiUrl, parameters, 10000);
            if(result != null)
            {
                JObject ret = JObject.Parse(result);
                int code = ret.GetValue("code").ToObject<Int32>();
                String msg = ret.GetValue("msg").ToObject<String>();
                if (code == 200)
                {
                    JArray array = (JArray)ret.SelectToken("result");
                    foreach (var item in array)
                    {
                        JObject tmp = (JObject)item;
                        String callback = tmp.GetValue("callback").ToObject<String>();
                        String taskId = tmp.GetValue("taskId").ToObject<String>();
                        int status = tmp.GetValue("status").ToObject<Int32>();
                        int censorSource = tmp.GetValue("censorSource").ToObject<Int32>();
                        int callbackStatus = tmp.GetValue("callbackStatus").ToObject<Int32>();
                        int riskLevel = tmp.GetValue("riskLevel").ToObject<Int32>();
                        int riskScore = tmp.GetValue("riskScore").ToObject<Int32>();
                        long duration = tmp.GetValue("duration").ToObject<long>();
                        String dataId = tmp.GetValue("dataId").ToObject<String>();
                         Console.WriteLine(String.Format("taskId:{0}, dataId:{1}, 回调信息:{2}, 状态:{3}, 审核来源={4}, 回调状态{5}, 风险等级{6}, 风险评分{7}, 时长{8}", taskId, dataId, callback, status, censorSource, callbackStatus, riskLevel, riskScore, duration));
                        // 机审结果
                        if(null != tmp["evidences"]){
                            JObject evidences = tmp.GetValue("evidences").ToObject<JObject>();
                            Console.WriteLine(String.Format("机审信息: {0}", evidences));
                        }else if(null != tmp["reviewEvidences"]){
                            JObject reviewEvidences = tmp.GetValue("reviewEvidences").ToObject<JObject>();
                            Console.WriteLine(String.Format("人审信息: {0}", reviewEvidences));
                        }else {
                            Console.WriteLine(String.Format("Invalid Result: {0}", tmp));
                        }
                        

                    }
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
