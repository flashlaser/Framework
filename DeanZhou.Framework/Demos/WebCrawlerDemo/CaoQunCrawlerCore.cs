﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace DeanZhou.Framework
{
    public class CaoQunCrawlerCore
    {

        private const string CaoQunUrlTemplate = "http://ac168.info/bt/thread.php?fid={1}&page={0}";

        public string CaoQunUrl
        {
            get;
            set;
        }

        public CaoQunCrawlerCore(int pageIndex, int t)
        {
            CaoQunUrl = string.Format(CaoQunUrlTemplate, pageIndex, t);
        }

        private List<CaoQunMainItem> GetCaoQunMainItems()
        {
            List<CaoQunMainItem> res = new List<CaoQunMainItem>();
            HttpCore hc = new HttpCore();
            hc.SetUrl(CaoQunUrl);
            string mainHtml = hc.GetHtml();
            HtmlNodeCollection mainItems = hc.SelectNodes(mainHtml, "//*[@id='ajaxtable']/tbody[1]/tr[@class='tr3 t_one']");
            if (mainItems == null)
            {
                return new List<CaoQunMainItem>();
            }
            foreach (HtmlNode mainItem in mainItems)
            {
                var an = hc.SelectSingleNode(mainItem, "/td[2]/h3/a");

                string infoUrl = "http://ac168.info/bt/" + an.Attributes["href"].Value;
                string title = an.InnerText;

                CaoQunMainItem temp = new CaoQunMainItem
                {
                    CaoQunDetailItem = new CaoQunDetailItem(),
                    InfoUrl = infoUrl,
                    Title = title,
                    Url = CaoQunUrl
                };
                res.Add(temp);
            }
            return res;
        }

        public void ExecCrawler()
        {
            List<CaoQunMainItem> mainItems = GetCaoQunMainItems();

            Parallel.ForEach(mainItems, caoQunMainItem =>
            {
                HttpCore hc1 = new HttpCore();
                hc1.SetUrl(caoQunMainItem.InfoUrl);
                string infoHtml = hc1.GetHtml();
                HtmlNodeCollection imgNodes = hc1.SelectNodes(infoHtml, "//*[@id='read_tpc']/img");
                if (imgNodes == null)
                {
                    return;
                }

                string remark = hc1.SelectNodes(infoHtml, "//*[@id='read_tpc']")[0].InnerText;
                caoQunMainItem.Remark = remark;
                Parallel.ForEach(imgNodes, htmlNode =>
                {
                    string imgUrl = htmlNode.Attributes["src"].Value;
                    HttpCore hc = new HttpCore { CurrentHttpItem = { ResultType = ResultType.Byte } };
                    hc.SetUrl(imgUrl);
                    HttpResult temp = hc.GetHttpResult();
                    //把得到的Byte转成图片
                    Image img = temp.ResultByte.ByteArrayToImage();
                    if (img == null)
                    {
                        return;
                    }
                    caoQunMainItem.CaoQunDetailItem.PicUrl = imgUrl;
                    try
                    {
                        if (!Directory.Exists("F://imgs2/" + caoQunMainItem.Title + "/"))
                        {
                            Directory.CreateDirectory("F://imgs2/" + caoQunMainItem.Title + "/");
                        }
                        string pic = caoQunMainItem.CaoQunDetailItem.PicDic = "F://imgs2/" + caoQunMainItem.Title + "/" + imgUrl.Split('/').Last();

                        Console.WriteLine(imgUrl);
                        img.Save(pic.Trim('?'));
                        img.Dispose();
                    }
                    catch (Exception)
                    {

                    }
                });


                //数据入库
                DapperHelper dh = DapperHelper.GetInstance("Data Source=.;Initial Catalog=DeanDB;Integrated Security=True");
                const string sqlFormat =
@"INSERT INTO [dbo].[WebCrawlerResult]
           ([Title]
           ,[PicDic]
           ,[Url]
           ,[Remark])
     VALUES
           ('{0}'
           ,'{1}'
           ,'{2}'
           ,'{3}')";
                string sql = string.Format(sqlFormat, caoQunMainItem.Title, caoQunMainItem.CaoQunDetailItem.PicDic,
                    caoQunMainItem.InfoUrl, caoQunMainItem.Remark);
                dh.Execute(sql);
            });
            foreach (CaoQunMainItem caoQunMainItem in mainItems)
            {
                
            }
        }

    }

    public class CaoQunMainItem
    {
        public string InfoUrl { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public string Remark { get; set; }

        public CaoQunDetailItem CaoQunDetailItem { get; set; }
    }

    public class CaoQunDetailItem
    {
        public string PicDic { get; set; }

        public string PicUrl { get; set; }
    }
}
