﻿// -----------------------------------------------------------------------
//  <copyright file="ValidateCoder.cs" company="OSharp开源团队">
//      Copyright (c) 2014-2018 OSharp. All rights reserved.
//  </copyright>
//  <site>http://www.osharp.org</site>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2018-06-28 22:31</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using OSharp.Collections;
using OSharp.Extensions;


namespace OSharp.Drawing
{
    /// <summary>
    /// 验证码生成类
    /// </summary>
    public class ValidateCoder
    {
        private static readonly Random Random = new Random();

        /// <summary>
        /// 初始化<see cref="ValidateCoder"/>类的新实例
        /// </summary>
        public ValidateCoder()
        {
            FontNames = new List<string> { "Arial", "Batang", "Buxton Sketch", "David", "SketchFlow Print" };
            FontNamesForHanzi = new List<string> { "宋体", "幼圆", "楷体", "仿宋", "隶书", "黑体" };
            FontSize = 20;
            FontWidth = FontSize;
            BgColor = Color.FromArgb(240, 240, 240);
            RandomPointPercent = 0;
        }

        #region 属性

        /// <summary>
        /// 获取或设置 字体名称集合
        /// </summary>
        public List<string> FontNames { get; set; }

        /// <summary>
        /// 获取或设置 汉字字体名称集合
        /// </summary>
        public List<string> FontNamesForHanzi { get; set; }

        /// <summary>
        /// 获取或设置 字体大小
        /// </summary>
        public int FontSize { get; set; }

        /// <summary>
        /// 获取或设置 字体宽度
        /// </summary>
        public int FontWidth { get; set; }

        /// <summary>
        /// 获取或设置 图片高度
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 获取或设置 背景颜色
        /// </summary>
        public Color BgColor { get; set; }

        /// <summary>
        /// 获取或设置 是否有边框
        /// </summary>
        public bool HasBorder { get; set; }

        /// <summary>
        /// 获取或设置 是否随机位置
        /// </summary>
        public bool RandomPosition { get; set; }

        /// <summary>
        /// 获取或设置 是否随机字体颜色
        /// </summary>
        public bool RandomColor { get; set; }

        /// <summary>
        /// 获取或设置 是否随机倾斜字体
        /// </summary>
        public bool RandomItalic { get; set; }

        /// <summary>
        /// 获取或设置 随机干扰点百分比（百分数形式）
        /// </summary>
        public double RandomPointPercent { get; set; }

        /// <summary>
        /// 获取或设置 随机干扰线数量
        /// </summary>
        public int RandomLineCount { get; set; }

        #endregion

        #region 公共方法

        /// <summary>
        /// 获取指定长度的验证码字符串
        /// </summary>
        public string GetCode(int length, ValidateCodeType codeType = ValidateCodeType.NumberAndLetter)
        {
            length.CheckGreaterThan("length", 0);

            switch (codeType)
            {
                case ValidateCodeType.Number:
                    return GetRandomNums(length);
                case ValidateCodeType.Hanzi:
                    return GetRandomHanzis(length);
                default:
                    return GetRandomNumsAndLetters(length);
            }
        }

        /// <summary>
        /// 获取指定字符串的验证码图片
        /// </summary>
        public Bitmap CreateImage(string code, ValidateCodeType codeType)
        {
            code.CheckNotNullOrEmpty("code");

            int width = FontWidth * code.Length + FontWidth;
            int height = FontSize + FontSize / 2;
            const int flag = 255 / 2;
            bool isBgLight = (BgColor.R + BgColor.G + BgColor.B) / 3 > flag;
            Bitmap image = new Bitmap(width, height);
            Graphics graph = Graphics.FromImage(image);
            graph.Clear(BgColor);
            Brush brush = new SolidBrush(Color.FromArgb(255 - BgColor.R, 255 - BgColor.G, 255 - BgColor.B));
            int x, y = 3;
            if (HasBorder)
            {
                graph.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);
            }

            Random rnd = Random;

            //绘制干扰线
            for (int i = 0; i < RandomLineCount; i++)
            {
                x = rnd.Next(image.Width);
                y = rnd.Next(image.Height);
                int m = rnd.Next(image.Width);
                int n = rnd.Next(image.Height);
                Color lineColor = !RandomColor
                    ? Color.FromArgb(90, 90, 90)
                    : isBgLight
                        ? Color.FromArgb(rnd.Next(130, 200), rnd.Next(130, 200), rnd.Next(130, 200))
                        : Color.FromArgb(rnd.Next(70, 150), rnd.Next(70, 150), rnd.Next(70, 150));
                Pen pen = new Pen(lineColor, 2);
                graph.DrawLine(pen, x, y, m, n);
            }

            //绘制干扰点
            for (int i = 0; i < (int)(image.Width * image.Height * RandomPointPercent / 100); i++)
            {
                x = rnd.Next(image.Width);
                y = rnd.Next(image.Height);
                Color pointColor = isBgLight
                    ? Color.FromArgb(rnd.Next(30, 80), rnd.Next(30, 80), rnd.Next(30, 80))
                    : Color.FromArgb(rnd.Next(150, 200), rnd.Next(150, 200), rnd.Next(150, 200));
                image.SetPixel(x, y, pointColor);
            }

            //绘制文字
            for (int i = 0; i < code.Length; i++)
            {
                rnd = Random;
                x = FontWidth / 4 + FontWidth * i;
                if (RandomPosition)
                {
                    x = rnd.Next(FontWidth / 4) + FontWidth * i;
                    y = rnd.Next(image.Height / 5);
                }
                PointF point = new PointF(x, y);
                if (RandomColor)
                {
                    int r, g, b;
                    if (!isBgLight)
                    {
                        r = rnd.Next(255 - BgColor.R);
                        g = rnd.Next(255 - BgColor.G);
                        b = rnd.Next(255 - BgColor.B);
                        if ((r + g + b) / 3 < flag)
                        {
                            r = 255 - r;
                            g = 255 - g;
                            b = 255 - b;
                        }
                    }
                    else
                    {
                        r = rnd.Next(BgColor.R);
                        g = rnd.Next(BgColor.G);
                        b = rnd.Next(BgColor.B);
                        if ((r + g + b) / 3 > flag)
                        {
                            r = 255 - r;
                            g = 255 - g;
                            b = 255 - b;
                        }
                    }
                    brush = new SolidBrush(Color.FromArgb(r, g, b));
                }
                string fontName = codeType == ValidateCodeType.Hanzi
                    ? FontNamesForHanzi[rnd.Next(FontNamesForHanzi.Count)]
                    : FontNames[rnd.Next(FontNames.Count)];
                Font font = new Font(fontName, FontSize, FontStyle.Bold);
                if (RandomItalic)
                {
                    graph.TranslateTransform(0, 0);
                    Matrix transform = graph.Transform;
                    transform.Shear(Convert.ToSingle(rnd.Next(2, 9) / 10d - 0.5), 0.001f);
                    graph.Transform = transform;
                }
                graph.DrawString(code.Substring(i, 1), font, brush, point);
                graph.ResetTransform();
            }

            return image;
        }

        /// <summary>
        /// 获取指定长度的验证码图片
        /// </summary>
        public Bitmap CreateImage(int length, out string code, ValidateCodeType codeType = ValidateCodeType.NumberAndLetter)
        {
            length.CheckGreaterThan("length", 0);

            length = length < 1 ? 1 : length;
            switch (codeType)
            {
                case ValidateCodeType.Number:
                    code = GetRandomNums(length);
                    break;
                case ValidateCodeType.Hanzi:
                    code = GetRandomHanzis(length);
                    break;
                default:
                    code = GetRandomNumsAndLetters(length);
                    break;
            }
            if (code.Length > length)
            {
                code = code.Substring(0, length);
            }
            return CreateImage(code, codeType);
        }

        #endregion

        #region 私有方法

        private static string GetRandomNums(int length)
        {
            int[] ints = new int[length];
            for (int i = 0; i < length; i++)
            {
                ints[i] = Random.Next(0, 9);
            }
            return ints.ExpandAndToString("");
        }

        private static string GetRandomNumsAndLetters(int length)
        {
            const string allChar = "2,3,4,5,6,7,8,9," +
                "A,B,C,D,E,F,G,H,J,K,M,N,P,Q,R,S,T,U,V,W,X,Y,Z," +
                "a,b,c,d,e,f,g,h,k,m,n,p,q,r,s,t,u,v,w,x,y,z";
            string[] allChars = allChar.Split(',');
            List<string> result = new List<string>();
            while (result.Count < length)
            {
                int index = Random.Next(allChars.Length);
                string c = allChars[index];
                result.Add(c);
            }
            return result.ExpandAndToString("");
        }

        /// <summary>
        /// 获取汉字验证码
        /// </summary>
        /// <param name="length">验证码长度</param>
        /// <returns></returns>
        private static string GetRandomHanzis(int length)
        {
            //汉字编码的组成元素，十六进制数
            string[] baseStrs = "0,1,2,3,4,5,6,7,8,9,a,b,c,d,e,f".Split(',');
            Encoding encoding = Encoding.GetEncoding("GB2312");
            string result = null;

            //每循环一次产生一个含两个元素的十六进制字节数组，并放入bytes数组中
            //汉字由四个区位码组成，1、2位作为字节数组的第一个元素，3、4位作为第二个元素
            for (int i = 0; i < length; i++)
            {
                Random rnd = Random;
                int index1 = rnd.Next(11, 14);
                string str1 = baseStrs[index1];

                int index2 = index1 == 13 ? rnd.Next(0, 7) : rnd.Next(0, 16);
                string str2 = baseStrs[index2];

                int index3 = rnd.Next(10, 16);
                string str3 = baseStrs[index3];

                int index4 = index3 == 10 ? rnd.Next(1, 16) : (index3 == 15 ? rnd.Next(0, 15) : rnd.Next(0, 16));
                string str4 = baseStrs[index4];

                //定义两个字节变量存储产生的随机汉字区位码
                byte b1 = Convert.ToByte(str1 + str2, 16);
                byte b2 = Convert.ToByte(str3 + str4, 16);
                byte[] bs = { b1, b2 };

                result += encoding.GetString(bs);
            }
            return result;
        }

        public static string CodeList => "{\"wQT3\":\"ZGF0YTppbWFnZS9wbmc7YmFzZTY0LGlWQk9SdzBLR2dvQUFBQU5TVWhFVWdBQUFHUUFBQUFlQ0FZQUFBRGFXN3Z6QUFBQUFYTlNSMElBcnM0YzZRQUFBQVJuUVUxQkFBQ3hqd3Y4WVFVQUFBQUpjRWhaY3dBQURzTUFBQTdEQWNkdnFHUUFBQWl2U1VSQlZHaER0WnBaanh6VkZjZjlJWkFnRCtsbGVtWjYzNWZwcGFhWDZwN3FiYnBiaURBakkvR0Fvd2hRRWhMWmlFaVJVQkxIbXlLUnA0aVJTT1RZc1pWRVJqQXhoRkVNSnRqS09MYmdBUlFzRm8zellna1E4c3Q4ZzBQWGN1ODl0K3JXTWozdGg1K3ErcHphK3Z6cm5uT1hPcksvdncreGRoSFVyWWprUkJiYXZSSWRWb1YyTTNLcUk3UTcwVWlPaEhZdkRCTitvZDJOY3FBbHREdmhXNnpRL1hZcnhmbk1hSUlRR3Y0WTV6d3MwbkJpc1EwYjlnSkgvUXN3amljaEZWd1ErbWRoZFp3UjJrVWt4ajd1ZHppUjBMYVpjbzJ6ZXlYYlhORzJDNTExYW92bm0zUy9WVS9UZlFJbkNNYlhpQXJ0WGxDaVplalVVMURvOXFpdFhxeHp4NmpJMlFMZEw4bjYvYUpkaGRvd0pmOVFhRGNUek9TRWRpZDZNZ3RZSWNTTFlrY296QUxyUmpHeExMU3JqTVloN3JldElQTWdsbDJHMmlBcDlObngvaXQvRmpLT3hJVjJOL0MxWTlJUzl6dGVyVUdwV3Vac1hvbkU5R3N0NWF4WlpkS3p2bnhlOFNTSUVtWnZUU200eXZubWlUbVlzWDdJWXBzbkhYa2t0RHNSOUdVdHo5MWZzOWJnVW5tMnRIdW9GdEpQNU9sK3ZCS2crMUxCVzdNL0xCTWp4enRCQXVrclo3bkFpaGdwVVJoTENZdTlJaWtXR3lZaDk0UjJyK0RuUFhUS0NtWVd1ZCt0bUxkZWxSdjVybnNQcWxQMG5zZm53VUp5VlJoUVRIRVFFTnBGTkFkcjJoYmZReWpJSktzM3kyR2hZZkdaQ2NaWnkyamJGR1JDck1XTHRiWjY4QUxzQmFVNFkxcjk5Q3A4K2NpemNOZk04ZHZDNHpNaHZZNzg5KzJUMEMvbnRTM0JmQ3hocWNJSy9Hb3ZDNDNGTU9jL1ZBc1oxK3g3RHcrRGNTa290TS9LVW9EMUFuLzFQWUVRSEQrQWI5QzVPUGdZNHNjb3N2UFlBM01nUVdvRjFrVXIxT2FUbWxSNjNaSzI5ZWY0UWpoSXQyQnhKSEUyUWpOcDdkM2tWK3hUMk1XemI5cHlaZU5GZ1FBQ3BpM0Zxd2hlYWVaNXNRNWRROHpJbWJIUUxpSW9EOUR2dTNEdjJLTndSM3JNeEFtNGo4NVJFUVYxZHJaZ0Z3Yzk5UW84czFEV2ZQdjd0K0gvMlBmSXorR2pRNGlRQzRsYlNyVEE0bUFyU0d2a1hEOEdjWDVBWTBmWDUxeFhWTzZmRWdsaDVlT0xkN1hqczQxbEZORHo4TzVSQ2Y0M21aSDZVeWpnSitIK3Avb3prYmYvb3cwc3lFK29QK0Jmbzg4L3psVmdZV0U4SFpsN0c3eUdJdFpwb21SY3IzdHpheUhGcFI2VUVtems3Uld2WWpDZWdPMmJmNE5MbEMxNDcwZUNRQitBZXpmMFp3bkpCYWhWZzFRTXF5Q3ZjWFVrSEtuQXVYZmVweEM3U3JySWhnUWkxaXZpTHZ1QkJaRWI0cHp1aFVLUmRXVmJBeG4ycjU5d0NUYmgxM0JkNG9XN1VBbHl4K0Q3RUhxTkdndjgrQlI4alh6SkladEh3Z0pnOXQ5NURZbnhMUHk3V0tQQjcxV0huQmdxNUhyTGE3UFhWMDZRMm9veEdiajNDZlNlZVJNVWczTzMyREVxM1VnZkxyOHM5dGZEYWo3OGl2TS9kNWtGVHVsbjZmNzJ6M0NRN2NSQXdUWUorTmwxZGw4em1XRU1Idno5R0JLa3hnbWlJaFRCVWpjTU5pL1pDakF2K21XL1F3dTV0VXNEcWp6eHRNbS9CK2VJVCtYMEZTNkF3Y0VQNFRucWZ4M09JaC9qRDdDenlRUWg5ZUhHaFROQ3BQWlEyOFk2Q1NUS090ejdIRDhYRHkrSTNrTEVJckJ6U3EwKzNER0pjZWY0UDBCUkpuTVJvUnhpNHpZUlFrRnlXazlweno2b2IxL2hXcERJZjVUNFh2NEV2aERjbzVaZmhzOVFHanJmQ1ZwRXNPUG1pU3dUNWZGTnVHbllhOGtPdEFwK2JWKzlCeS9JQm56b0lBUjk4MzkzaGhPRE11M3lTaVcrR01mYnM2MnBpQmo3OVM2L1l3MDVlNW9GSGFlZFM1ZGZwM1lDbDladzY5cmE0NjdKNEx1NU9QMzBsQlRrSS9ickdEY3UvQUoySDJmbjN2cnRHV2lHRmNqMngxUTBsZi84Y21nUmhQaW9BQVpocVVEMzMvalhhWTEzai9LaWZQbnFQZUh6aUVpMzRrSzdTcUJ2UDE2aWdvelFtZ0JsV2t1ZUZ3UTJ2M25lQ1BndW5OdmkvZXZTdEhBalFaN2Yvb3BkajhOZUVFL2dlbkpzQzc1RnZwS3ZJeEJrRS82Q0JEQ1RYSTVBV2dwUU1YVGhUc0tIUlN3SzM4dDZHRmhhaUp6Q2d4ZGNuSGZoQThQK0FSRkJUVWMwK05mZzhwN3UvMkw3bW1IVFcwN04xMGZYWkh4N2NaMEY5ZFFPdFhkU1hlNDRNVHNvNWJGYTBrNnhWY3FuSzRZWW1pQXZXQVRKcGZNbytJekp1RXozYjQreElQbzRwVjdpSjFUZEdMWEVLN0U1bjNXSzNqRmxxWmlEaTBYUzMzNVc0RWxyR0J6VmYyTVJWU3JtdFh1SHQ5d0xlQXhET2dVcTVDMy81MW5jUWw2QWE0WWRYOE9OYjQ1alFieTFrR0hJMndEUmpETHE4SUkwMHFKRmxUM29HQUhYMHhJVGdOUU5yc1hnVm1WVDBMT2xKZkEzMVFKNXVMUlZsK0wwM0R0UGJzQmJSc0RGZ3ZEakVCVzNZR2Q3ZGI0TFBMZ0tEd3hmZlgyMm1lcEdVNC94SWpkdHhIQnRJU3Bjd0dsZDBWTlVLemd0WHRUMng2a05DV0piMEJtaXROVUo2Zk5oL2I3MTR3SVM3T2hpSGQ3NDZ3YnMwclRWZ1IzRFI0NFZkWHZ4dFI2OGVoSUpZaTNhdkdDOFA3ZnVmUVoza0dlenltRS92MlRjcnJNcEdCV0xJQXVpejNad25kZ20remdkWVJHbWhWN3ppd3Q2TW1oZXc4YTE0REg0VTU3L1VvVUlnQ25MY1gyZkU4UTZDZWttaURvSXZJSUM3c2lqSTlvNlZOSUI5NDlBcEZWV2F4cDVieCtOSEVuRXJOMUx5VytlaDhIRjNjQ1VqbkN0SVhCZFlTZTRFVGg3MDYyOEJPODlPUldBZEFDNDgzaEJCdkdXQjBHbW1LWkhoS0JVaGVuUCtIMUJzMm8vNTZlMWtGNVcvMzdJQ1V2QXpla0lkNUUxZHFHNXpLK3RKL1BXdFhZUzdMZCtnd1o3aGlqdFpGVHprV081OUdaRzBDbndKSWlLM1VyaEZLZXh4K0pEV0VMbWkvcUU1YnBpMU54VjVhZExST21JMWhxVmFRdGFWMVlnWEdiWFZDRUNkQ005dWsvWStURkxYU3FreUVmS2JJeGtKd3J1WlJHY0JBbVY5QlJTSGJXcHpZbW96WGRZNGFMN3kreVZ0Y1VsT0xKYTRMOVZJbFNiMWxTR0F5NU1SNllSdWp6eFdZSnVaaVhCMzBjWThNYjMyZHYvK1JaY1JUV0g0RVdRbjRZam1qMFhzUTZDRzYzWnZwUUo5L2cxY1RlVWxQUDZrS2RlbGh0ckxmMlBxcEJBNTZhMUNRZWVnTSt6STFQendjZUNvTHZEMXhIUEtRc1J5M2dabERxVFNJc0hqbW5GZmU3cndJTEVZaVdvUlBnMWtVaDdDTEcxTUJTR0NTM28xVmpCc3dDanJzT2JPVzBOTzA3Q2FIWGpMdnkreGg5RFdzc3NncWdFSmpKRWZQUDdaa0RGMTNTZTVkWFpoKzhBSHo1R3NrdUVMQU1BQUFBQVNVVk9SSzVDWUlJPSMkIzhmNWI3MzQ4YjEzNjQ1MDY5ZmE1MDQ3MmI1NGIzZGQ3\",\"8wnv\":\"ZGF0YTppbWFnZS9wbmc7YmFzZTY0LGlWQk9SdzBLR2dvQUFBQU5TVWhFVWdBQUFHUUFBQUFlQ0FZQUFBRGFXN3Z6QUFBQUFYTlNSMElBcnM0YzZRQUFBQVJuUVUxQkFBQ3hqd3Y4WVFVQUFBQUpjRWhaY3dBQURzTUFBQTdEQWNkdnFHUUFBQWxmU1VSQlZHaER4WnBiYXh4SEZzZG5QOEcrN2VOY3JKbWVHV2xHbzVuUjNPK2FpK2JTSTJFVU1BVGpzTVNPbndUMmk5ZG1FMVpya0JON2c1Y1ZScUJoSGJPT0VMRlpYd0lPS0dBSGExZUlMRXZBd3NqQ3hJUUVySmY0UmQvZ2JOK3E2MVIxelUwWDl1RkhkNStxYm1uT3YrcWNVOVZ0MjkvZmg2TmdNbFUzenlkQ1JhYnRLR2lWVHdqdElocEJyOURPTXlKTkN1M0hRY1F6YnA3N1pKbHB3MWdFbWFoT20rZXVHblZ5THlScHNINHFFU2tudEhjajJCb0RyNjhpYkZPSmxZUG1lU3Mrb1IzTGhlNC8rS2p4MTh0Q2V6Y0trWWpRVHJCVi9jYzNTakt1TUxTcmRvczkwcUNqNVRqeFZMc0xPU3lsYUVwbzU3Rm45ZDhyVngzYVVmTDdtWFpNd3BkZ3JndmhzSFdHREVMQTR4VGFENFBiMnpza1RjcGhvYjJTbUJYYWo1cTYxOFZjWjRwMXlFUlpodzVDTGx2U2pvVnFIWjdmM3dEL3pKaDJWRkh0TmpuS2ptRGZySzRzVHloRlE0MFVIbTdrQlp4ZVNEaDdqN0I4dGJmSWsrNkFlVzdQOWc4VG9hSjFadjYvVVowZW41azBCVkJ4MlRQbXVkcm55Skk2SnRKMlFNRk9uUlpNVlpsMlFuS2NqYWVMSzE5cllOdHhNZTFNZ2lOQVJSNFdPVXR6YlRldzR6SDFtYVoyRk4xakNoSm9pcDAyT1psbHJrT0pVZWFha0NzMWhYYWUySXgxZEJNaGhoV2thRTlxeDBSaXl0TFdqVmlaRFQyYUxaZzJ6eE9WQXRPbWtndGFmM05qUXJMWVZIam5lM0lURUtvWHV3clFpdm1ZYTFPUWFxQjM5ajh1UkVLMFc3cWpDVEVQVzhiNjJ0UVo1YnkxYWl1bnJZbDArYzQvTkFwU3d6em4rNmhJOXBiUXpwTWUwVVZNbkNoWlJNQTAyM3JSTkNMTk1QZmIwK0xDUmhpeVRnU3BReWFDYnFadEVFSnhObDgwdy9vSTg0L1FrWWFGd0dLSUNEajBoRjZUMmRIVUMrTDBYb2p1NjRmSTZTcVZzS3dkUmZkZ1pJYzEvNDRXcVZoRDVaQmFldkQxUmk5RVFyamJYbWo3UjVoK2hJS2o5N3FpRXBHRUR1Y1IzY3VUZGJFaldjVVp5bW5PUGlHVndWdWRZSVRnK3g2RWRKVldtTGJRRkozeXJsVE5QQmV6QjgvbnZvSWJIZ0dYWHd2NnMyQWhzQmoyZ0Q0TGc3SGVpMGJITEExVklvZGoxanUzR2ZCekNEbEpYR3BqaHhOU1VZZDV6dmRQeWZxQ2xEQXFIM3lkTmZBTStibnpSQ3dFeCszT250WS9PRDBHdTNlMm9SRC9yMDd6Ty9nako4UjA3ZSswL2N4UHNHdllUWjd1MHZiNE0vaFk0SGdDTHdEQlcvSnJSL1Y1d1dLRGZiNUN0QkZqSEMrQ3YrY29rQUxpV1QrWUlEcy93RzNzK0xrZjRHZXpuWjgxVCtENWpsTFMrcFVGMjZ1ZlFEWWQraSs0Z01SUWViWkEybFIyNFJscVU1Mzg4Um5hL3NHZkJ4T0FnUCtPQ09Mc1pGVXZRVEhPMXVDNXFoc0ZaMXRveDdUTGVtN011MmtsYS9NNSs5ZlRMeTVqaDIvQUMwdWYxL0RJYkZjd3d0Zml5amR3cmttY3VnM3hPSzVnM3NLaUtZYmVYb3JRcEw5ODV4RjhZTFk5ZzF1Y3c1dkpHSE5Obnl1R2Qzb3N3MVpIYTkvOFZZUDBEMHdOWDh6TSt0Z2xBa0ZxZW9SMkVlSXFLNHRqT1RjRDBPeVk4cEZSd1BYSlA0QnJSbmpLSjZqRDc3NGl6OXhudzVuQjRsUGF2djdKVTlOK2FuN1ZkSHlzRm1FRUtJekVsT01Pdkh4SDdsMkgrVzBKZnFOZzIxelduRjBMMHUwSlFzdEY4NUVxaEwyVU5VV3BWOFJyREpXeVI4OFgwYXo2ZDhWOUNQSFU4RnNybWlENWtuaTdoTURNRVA5bEZLNEl2R2dQelZ5QkhVOGQvaXZjSmVGb1lkZWNLZGp4dDA0YjdmRk51UHE1YUFic3dOSXJ3L0hiNDdEMGJoMU9HVUx3eEwvN0owakIvbHNwUkJDTXFOOTBNQ3EwODJURDdNeW9lL3BYcVFmS0lZL1dsWWNudmVDVTlCaklKdno3OE5sMVhZenB0QklPVVdJK2UrZFgvWGxLYmpsbDJPWS9XWVdySkt5ZGZtQUk4b0NHTTVUc3F5UDRCM1VYZ01lMi9aNGlHTG12TzhueVZUaDk4aUdjSTF4YXNRZ3pHbForOCtzWHNJRDczZnlSZWM1aE9IQ1ZSYXFwYm5ZVHhmbG5pWE1OaDYvTmIrclg4VFV0TjVpemdUZ2ZpWWpER0JOMnZ2NFR4RG5IeisvcC9hWUNhaXlub1F1MzllUE52VytwbzA5dXdoS2FMU291T2NIMXVRNWJndWNjbElFRW1Vb2FWUWRmYlhHb000ZmNRMExQZWdmTmdQaThOdnJuRFdmbkErZTF2alNzNlpVV3JiNTJvWU5Gd0t5Zk01MXQyNzRDcWZvRUpKMXNLSG55Wm5oQitORy9zcUhicVNpMzROT3p0SDNZMmVGcDVZVjJ3c0F6UktYZldtVHAvQmRJQ0lvNUE2Si9nMTF6OUc5RHRHbThIRU8yYTUxTnVLQ2RLM3k0YVJGaXRLWnY2L1IzTnM0eGc0VXNuVDE0ZkpFNi9FeHpCdVF4dXZHNnR2WUZYQ1JpS0tpQ3BVdTluVHdNQXdyQ2xiWEkrZmNzSy9kVnVLZll2Vlc2dFkxbndDSVovYWtsTFR4cGp1NXN3ZnVHQ0JjKy9ON28reDlGbkEwSWpJMmgvNE13aUxOUnRhWE1vQ2VvYmJUZGZVZGlkclRDaHFTTEwrQU43ck94U2R0T3Jwb2hEVDlEUlpZUHRzM1VVeEE1RlZLTy9NSnZEZTdlNEdiQmpTOTdMQndWbUJXM3p2dVgvbzFHUHBvVkJOSEszYVM3czAzZWRhQk0rcnpxUURIUGZuaVJrcTNiOEhZM0tlTi9oQlhUNmQvQzQ5ZTB6OVpOWW44SUMvZjJVQ2pUeVkxYWR3T0dvYXNncHJPdnJDSXh2b0xPRlNvRTdsL00vb1hwaC9OSkpmRXBUZXdhMzV1NVFlK0R5bUFEbk13Sm4xM2YwbG0rQ2hKeTlrdXVuMHJta2w5clZ5bi9zc08wdFV2aWR6b1kzdkc2dmJ0UXZEQXFwRTFFVUJKdnBES0NORE5CS29UQjQvTnJ5TkViTU9laTlYd05iUnNIMDZlWXNNWldXMjhoajV4ZHlGeEViVHE5dGxGVVRERVUvdkNvYVRyN3Q0dFJwbzF3b0lTT3dhR0pKRzVzVTBKWnFjMitPeG5Qancwc2lJcmI2d0JQaUw2N21VcEhXRUY0TWRURjNiV1A3ak9DV0xkTkNHeWU0Y3RmN0hEUjZHZFc3Z3R2b1p4eGdidkxseDdZMmZrdmRRSENiVGNTNURITWJlbnR0dTBtekMzcmR0R3pSUGppNm9jSWVEWnNhcVd0ZU5ZY0hHK0VmVXNiendkWVFYS051Q1lDaGk5MUxlc01BN1lDMHpjWVNWc3RlalR2VVhSd1FwZTZKdlM4SVpodCt6eDhaQXFsaXhMUGliOWc0Y0hKZldVRFYxKzZRS0o3WXFIRGZRWERDTUlJZ2V4blExUVFEZTdkQjd2NXFMZFBqOUg5c0dxejkrNXB5YysrWjA5RlBlQkdYN213aUJONmZCUTlnMHZvb2h3ekVHaE5zbkJ6azY1UGpCRDIrZk5GQ0RWa2FMZDgwR2dFdEd2TVNDQmlzZlZDZmFZbUNCYUNGME9IcjdSNm9GUlk5V3dOQ2lWOWtaWXBIZjY3cVFKK2NhVTQrM2NDWnlkaytuZGUvdktlMXE3Q0ovUmlaSmovaDEyVEVNaGlzZGpLQ1IzcmxzTE10VGRaWks2N29UN1RuQ0ZpSVhpczZ4RXNoSFhUY1RDcUZmR3U2RmpTT2t0Nk9ac2dTdWh4VisrdllxckdlMzhlZHB0RWhZYXJDWWRvamJRUCtkakJCeUVUc2daaEpEejRKemVISVpIcVg4K1BaN3FGTlNWaE92dC96QmVhRkwvQ2pVelRralNXK2oxSzdtd3lyN2pZRlhxaHhuN3RZaC82VTlaOStCOVkxcTdFT216R01nQUFBQUJKUlU1RXJrSmdnZz09IyQjZmY2NGJmZjRhNzNkNDMxZGIzNjcwODUyMDViMWQ5MjU=\",\"sJYq\":\"ZGF0YTppbWFnZS9wbmc7YmFzZTY0LGlWQk9SdzBLR2dvQUFBQU5TVWhFVWdBQUFHUUFBQUFlQ0FZQUFBRGFXN3Z6QUFBQUFYTlNSMElBcnM0YzZRQUFBQVJuUVUxQkFBQ3hqd3Y4WVFVQUFBQUpjRWhaY3dBQURzTUFBQTdEQWNkdnFHUUFBQWpvU1VSQlZHaER6WnI3anhQWEZjZFhxdmd6YXErOXR0ZnZ4OW9ldjNiOVdMOGZheE9xRUxWS2s0cWdvb1ltQ3R0U0FXa0RDS3FBUW9KU1VFQWtOSW9DVFVCSmxtcHBXSXBFbElic3IxRzBpUVRaL0pKSS9MNy93YWxuN3R5WmUrYmVzV2ZNMHZTSGoyYm0zRHN6OXYzT09mZWNPek8xdGJVRm81QVNIcUY5SE5saVF0dVB6MFZRbTVGKzBTMjA1L29TZExJcFladFZ2UDZNMEQ2S1hEOHF0RzhuNFJsSmFOY0VjYmlUeWpZZGRxSU9NdjUwQ1FwTzB2Ni9wT1FrUHpyYURtczJaMXdYK3YrTlVzMmFrUDBRR2N0ZVBxVFpFdTBGWmN0NWlMTVFRTWVQd2tmLytadTJQKzl0b0RaS01zNTdZQ3BWNUd4U3c4ZlpyRkt0T1lSMnYydnlheHJ4ZG1jNVcwUFNIeVNyYUlLMHNnT3U4VkdSQldGRmFaZURxTjJJdHpDNi9YRVJMV2M1Vzdha2g5R2l4RWVOUjJVaFVSYmF4ODRoZGlqNnlGT3lXS2twVzFtTWJKNjRvbDNpU1JjNkhnVDd5allaOHlMN1QwMmlSUDZmSjZULzNrRWpBMjNIWkNKdWl5RFJxQjRMWldhOVJIMmpoeGp4VGxzZjNHQmYvRVE5VGlRM0Rtbk5nampzaWdqV3lEeXhsTEUzNTIycmgwUnpjOXIrT0RIeXM1TmxiejhWMFRTWlc4UGhhV1RQNVdmUThTUmtPcnAzS1lLVVN5M1VnYVhmTFhDMmZJdWZkQWN6T01Nb045b2pCYkZMSXJKOWNYekJreGJhUnhGc2s1QVo4cE1uUDl4NVBGa244cENsVkJiQ0MwM1VRVVFvMDRhbG52a1RubzM3eG5xSUVVK3BCbFhubzJWNFg2N1hZTThLdy9XL3dEL2UvQWpxTmJQcjNvVUxiUDhoRnpaRi9YVGlSVDBLakNJN2p6M0pLbHpJQ3ZwejZKaVN6NVhCN3lsQU9UT3YyWHlTT0Q3YUZjTXVhWDhGSGRkejlJbS9EeXQzOEFEdnVmbzZyQTVGb2JSQ0ZaRGE2a08zK1NydXUvSXFmTWxjVjZhNU9Ib09rQ3BMMENoVWxmMW1ySTdhcG1Oa0RpbzV5UDB5d1NYVUxzTDJIQktLRU5jZFJTdlp0eUhJQS9qcStTZmc0NWJNQWZoakNZZVRTSU5rYktQSVI5bHFuQmZsd0h1WGtTaGhUMG5aWHJxSysrMVp2OHRjQjVQTzVvVjJTc0poZlVVZ05mQnp0blk5cm15Rmd0U1lGQzdpc2xaOXVwTTk2TmRKQVdibUlkRktFcGFxWFlQOWM3aW5pQ0h6Qm55SDJuaWlPWDcrb3ZUYU5EVHhvZWlWcis5ci9ZZ3dsK0gxNjdqUDBYZUlZTFNmR2QrZmV4bjIvdXhaMkx2alpiaXhvZG8yTnJsK2syRGJRMFI0blhwS2FqZGM5UU5wUzRKMHd6aE1qV05obUNZL3pReTJ6Rk8vWmpLaWgxZmdGZFMrSHk2cEhrVHBUcE5RcEhIekxTS0VnR00xVlNTWkYrL2g4MVI2Y1JMU3NwS2VQcWY3YlcxZnhySWdQVmNSQ2s2Y2NmWGNPR2JLMkJWazYvNDFXS09DUEg4TkhxcjJRQnduRjQ0cXJuVkVOTnM5WlR1Zm9tSFY2Q243WU9VaDZUdDNnclVQVWNNVkt3aWw3TzBOdCtmaCtBNHNnaWs3M29KMTVmN1dTREhUZ0NKSXN4S0I4Q0svNHBvTmtMaEdhVXN4ZlgrT3RFVnllTkliSjRoN21CakkyNHlreHR6UDNsQzlReGZFMjhWVk9xVlF5TURkZjc4N0V0cTNQbERUVXM0VGlDakdqTXdzdTlLRWVZSHhnR0dvb3Q1RSs2Mi95QXJ5U3kyVWpjSlIwZWZMZG8rc2lFOTFCb2FZL3VNWHNQdmlteEJRT2ZJTjB6WUdPOTRSYTZodXl3cHkrblBGMWtrTmhnTmFobkFXUHlRaUFXUm1CekdoWFdQdElQeUJHWHdlUHJ0eWRuR0kxT1lObWVwNVhTaUdEMzcxZ3RibjJMbko1aFExWlAwQWIzK29peUJpOS9vUHlna0xzM3BhblBQaldzU09JSlNIVnc5d2dsQmN2Z0M0ZmZiWHJvU2kzUHF0UUFpVjFaTmMvM2dHUDZpc0lMOEo3RkpzbkNqSFRzRXhHdFpxbjhEM3pQbVVwbWQwNkIwS3NnRkhCQUx3cklLN1o1N2h5TmdWSkYxTUlFSFdyajVRN0IrY1BtR0pUZ09IMUxGd2RRZmg1SzEzWVM1WjVFU1JpUlE5WlAvTW56UkI5ajc1bXRhTzc3RUpOMnBxSHlZRFl3bTJjUm9ma3ZEU3l4VHJHVng0UXVIcmZYajdSME03QXhWRFpycEpNb2N6ZDlZNXdsVzNzcTJWUE1yMnU5T3Fkd3k1OXhtNVZpS1QxUVpkN3NlS1lJVDlEWlRPa3JoMnljd08wM0lMeFNDRkRyckNsZU53a0Q3OTZlUHd4SHhTS01vdTkxT2FjRmJEVnBTWm02ZTBBZi93Qy9pVzZVUmd2V2NWYm5QdE9xeDNlR0pWSklJNS80TEx6K2lDbkkzTktQYmdJR2ZvdDg3ZGJ4eWRJdTg5SWJlY0xCZ0xSM05CS001Q2FiaGxuLzZuWWVlTVBvaUlqVS8wc0tXbXZ4Mi85U1doS2VvZE1uWW1jQ09zSU5aNXdBaXlEeTVmeHlKTWd2RWUxVGwrelczejYzMk1JQ1REcW9USjNPZ0ltcisvUVJQN3ozY0sreFR5UmRoRkJiR1ovc29nUVJCckc4SVRSRGhxTFVXTWVtNThyY0RpYkhTWm92QUFmSFZmM0U5R05QZ1VWNmtydEp0eSt3eE9oVWNzbVdEdXdVVnRzTVZ6aEF5YkFsKzhLZTRqVXdyeGRSeWFRNFFJUTVsOVdwRVphSlR3ZStkcXNXVnIyVVJFZnBhOHQwNFB5UEs4UE9DbGlvc1hnV0dtbm9aRC8yUUVXVGtJUndYOVpOaDdLYkRWK2pDVDJ1VTFoaU5HTkJtVHF0MGQxOC9yK1BRWFlkYlMzc043dEJQc2N1N0VPK1ljT1F6WHFDQy9PS3hWNlpSZ3p2cExySFIyL0dzRGxxTTNXVUdlZ1VPM2VURXFtWXF5WFVndklmdlIzY3lBNzFpR3cxZW9jQVl4bEhZK2JMbmE1bTgvaFVzbjM2Ni9qd1daMEV2a0FmODdNK0FYaklLODlIdlZPM0I3UHI2QSs2bUVuRDEwTE44ajdCTlg5VEtKQVY2TDh0WjEwZVovVjJZRXdZdVBsSkp6RVFuQlVwMHhERHpMY05MZm42Yjd5OUROQjdUejZMVWJRZkdpN1JTZEt4WjloamRnMzZ6cWdnejcrT2NuZU11VzYrZ0R6cXhUVVl3MUNEdlkyMEU3VmtiSDdMMjVkYTQ3VjJBVHRWdUF6YWdZOWkvZmdyOHVMNk5qVnN4d25QOWtpREpGSzNBanQ5ZDBENUd6cjNqSy9CdW10Zk9YRkdMU1FOdFhPSFVJVnFnZ3JXZGg5UlRUZHY0czh4NUVyMEZFTkVKNHZhemdJQy9KNUVIdU9lTm8wTzF3OGoxR2tKVW40Yyt2NFhiMm5yNGsrUTN6ZWY0Rlh0d25lTnBac1V5cWRoRktsc1dtdTc1c0JjOG5UTFlWQ2huZlplaGlpRGtKTnpSQkNDc3ZuWVdiejJIYng3c1B3YWZDOHduU2JKbXo1YndkN3JlTWd4M3MyTEJPWVk4emtZNjJINnhnenpLalg1RzAvVkxFT0RZV3F2WTgveTJEZWRvcll5UDFaY2ttOWU5VzBWcVZFT3c1bVlMQnkwWVFkZnM0VzZzZTVXeG1zTCtacGRjbFJSODcrRlpocjhQV0xWYXJkaE5CUmkrVGlDaEkvTmNwbEZwOXAwQUltY2xTWFJaNVlETmxzWWlaOUp6UXppSzZKaVhXY29FVXdCOUZENXJXcSs2MjFPZXE5dEZzd1g4QnRhdDA5UEJhZVkwQUFBQUFTVVZPUks1Q1lJST0jJCM2YTg2NDIxYTJhMTI0NTliYWE2YmUyMjVjNTBhMGI1Zg==\",\"ReSu\":\"ZGF0YTppbWFnZS9wbmc7YmFzZTY0LGlWQk9SdzBLR2dvQUFBQU5TVWhFVWdBQUFHUUFBQUFlQ0FZQUFBRGFXN3Z6QUFBQUFYTlNSMElBcnM0YzZRQUFBQVJuUVUxQkFBQ3hqd3Y4WVFVQUFBQUpjRWhaY3dBQURzTUFBQTdEQWNkdnFHUUFBQWpFU1VSQlZHaER2WnBiYnh2SEZjZjk2TDRXZ2YwUWxLSjRFeWx5SmE0bzhYNlRlQ2VYVkFQQkQ2N2J0Q2xVQXlsY3g1YmE5SUlXY0IyaGFlb3FVdUVrY29NbVJoRERoUldrVFFGRjZJTVRJRUhTbHo3a1FZRURQd1VJK2dIMERVNTNkaml6TTd0bmwwdGQvUEREanM0TWw5VDU3NXh6Wm1iUEhCNGVRbUMrRCtRYWFyYU02MHkxQWtzeHhXaVBZam8xaGRxUFN6eVFSdTFPOUNJQjFNN29LZ1hVZmhSaTRmSHZWUS80VUx2SVREQUhaOVM2SCsyMFVzZzJVUHRKczlqcm9uWXJvUVlWVEN1Yi8yZ2dxc0JTb1E2enBXVnVPeW1TVmVkN0p1c3p4alhmck5uNkdQM0ZXZDdPVkJlbFBoRmpoakNXRTBHcDh5VHBCTEtvblpGUHBsQTdvOUpNb3ZaeEtVdzVPK08wVWZJNW15MWE3NEphVytCL0c0S1VVcWFCa09qUTBPV1ZtY28wYi8velAyOUxmVjRwVGM0YjExeStiT3RqcEpKdDNpNDNhSml0TlhFaEUvRUlhdThublo5aWhoSTFQNnY1UjRjYUVmL1M4UjZjTStVRjB3R3Z2LzNPc1NCaUVKWWpDMmcvSTF2MG8zWjFwb0RhQ2VLUFBrMzY2UXBxUHdreXVSaHY1d3RtbTFIS05NeVFoVGxoWEpnZ1dOODQxRUt6cVAyMEdQakNxTjBMNVc3V3VJcU94YWhuelZ5ZHEwV2xQaEVwaHpEaURYdXMrK0xxYTdEeExRK2MzWUV2TEo4bHFObTR6U2FpelExUU84UHFDRWE0SGpXdXpmNDB0ODFQVHZMMlNiRFlHcUQyWnF6QzI5aHZadFRTM2dvbkFpb0l5dDRlTHNBSWRtNS9nOS92Q1pIWDVQeDQwc1JWK2Y3eFlra1hBTS9CMDM3M0NyS2w5SndGbWFqUVVzN2tBSGJQNGs0ZnlkVUQ0eDRwelN6OWxGaVl0OTFRMjBkWlB6eUVWOTlOd3pNT3ZQb1ZIUmVvcW5yNTdLM01mbEo0bmlHSmRrOEtXN3Q3OWpFc2h4aC9XMmZVVUpUVDVSRTgrQUFYQWVQNmZ4OGg5emdabEV3Q3ROU0V6VDRaVFJoWFg4NnNHQWxCbFJaWE5rR0NPVGwvQkxJOVdOQm83UjczdmVqcVlDYklSSkE5MWNLc09uc1BQanFReDR2NDVoVEloWTRUWHV4aTJCeit2N3R3WGVnbjlGNTBucW54WkFUNnVaTFJMcmFMdG40cjZlV3F6VGJJNTZHZGFrRktXTUFTRmpzZDZlK3lyNnN2aWx2NERPbVVTaEFzMG5XQmpPamdYOXVTdHpoRE9uTjE0L3IxN1h1U2lMTzFTZWt6VHRTME1yU0wza3JRZGtWLzJyNjZJVG1haFNVN1Z1RnV3Q2RDdjZKbGhMRlBIazhoSzVSWGVkc3RiREZCV2lwOXFnd09Qb01kTHVLNlRjVEFsTGxubGd6UjZYd1VQdmxZY1BJSGQrRXhNb2FqejVRdUY4UVVyenhpdDhDTmJwQStnRTYwWS9aK2JiNXBzeG1DcUJIdlQ0WDB4RDhsbDZyaURHSDBPcGVGV2JVTnV4ZS9EVzlrQ05maFFCakhTT3VMU3F2TkM4V2JnaUQ2VTE4dUw2SGpLSmFrLy9GRHc2NGt6QWZ2T095KzlBb0VaelBHbFlHTncvQ2MxRG5pRTkvNERMNFcrakJCdXVkZWd6OEUxYUVJT1BzUDlSQlZsNVBjT0ZTMEV2aC9saE1FT2QyRWJVVjBQQ0hVb1dMVVE0dmN4c2FtdXdYb0tYUmZiL3YrbTl6TzRJSkVOYS9iM2QvQVI0MmhJQ041Q1RaVlhBU1pEbnorQ1BzdVNsRnhmM0lyQlRLcm5Db3NPVWN3OXQvZjlzQUxjSEhsUEp3elNNSDZYYk92dTFBM3JwSHZKcURBeDNUZ0Z2K3NuYUtTUWUwTThydkdueUU2VXRoeTRVOXh3ZW5wSDhEdThJdWJpVG45ZWhYZUVVVlp2U3I5dUtNUTZvZmg1WHVZS0NKdGVQazkvUE1UVXgyYjdkYlBtYlBQdzhXL3lIMlVHN0MreXNiSW9vMEw4YTBoeVBLczg3WkdWVU1XWm1MWWN1THBqdWxzOVZsNC9TYnlJLzcyZlhpRmk5S0U5Nno5UjJiRGd6QTZ3OXhCYUZUcC84bENER2U5eVFVNXQvcThGSDRZQjdzYUg3UDJxZHpuQmdsWkxHekY4elIzb2pQRVh4bDFadUM4dnFCNTVCWjgva1BtYUJVMm42Smp4VzJVckxIZStSRDJ1U0R1WVlzUURnclZHMEk0NUxSMTc3Wmd2QUJidXBOTHRaUmREQjJ0RTRNZUUyUnRCeTFFRGo5ZE4wWGIydWYyelg5dnVDSUt3a0FGR1hUb1ltbTZhUjdQWmpOemtFK1pad25XOHRjL29PY1RoaUJ2YVVNbmp3TmVkYm1SbXZOK01yZzBZR0lpNHR5NXhnV3dmcTZkaWNBYWMvYktPdXdQN2N5cEE3VU9tdzh1Z01yR1hMNEF2eE9jN2tZNEVQRW1DT0d0OXpjNXpCYU0wOE9YZGtFdkFNU3RFV0hWVGdRaHg2cTQwOTA0eHdWWm1GcUE2Y1F5RlBvOWZsOUNGcW5iRFN5THdzWnowMUNZdHk5czg5a1FiMHZyRmlIeDJ4MzNDN2gwbVFreUM1Y2VXUHNKUDRFbUZ5ME9WM1FiK3g0M2xoTmQyeXhCQlJIRkVGR1h6SDlJRGx0N2NHbUdycXFKSUg5L0FYUDRLTDRqM051a05VVzNMUHdaYk9kZ2lHVkx4S25rbFp6NHJ4VkJrQVpjMjZQMnNLOGxqOU81OGl2bTdQUFEvQ3ZtN0MvaGpUVTJSbzhPajgyKzFFRGVJckZDeEpoS05zRS9UOHQrVkpCSnhUeUd4SVJoV01OV1JWRWdvR2JnM1F2TXlXWmVHT1FMOE9OaExpRjQzWllmVkwwc0ZCL0J0ZnVtSU0vYy94RnNXSnhxWmVNZkRVR1FGZmpOMEU3dVJ3NnN5RFVRenRQN0l6bWliamxaUEc1aVo3UEVNV1FSNm5INnBva1NwMDZ4Q25MbnA3ZTVnMW5ZS2t4RkJVSG9vby9kVDk1RytTM2M0N1BEUGFGYm5Za2lQZkU2dTFkNFg3T2lGeGI2MWJ3bnZsSjM1UEhPeU1SZUt6OXRFODFLWG8yRGI2RFo3SklnL3BpOU11a1U3TzlraFdiTXN3eUNJY3FkYmZnamMvRGNOdHpSYlNSa1BTY3VCbi8vb1RFK09BdzViQTBqcmQ2Zlh6VWRlMFRJdmVXOFFIZ1d0QlhMbXpTV2ZHTmRPR1o4MlBuSVBwcllKWFRSZkd5TVV6WG1BRHBEdEpwN1NXbGxva2oyaXVSVis1OXZVa0ZzVlpiazhGL0NsdGlYcWNQV3NDK1RLQXJqS05oM3UyRVh4UTE4RlkrUmJqQkJ6a052OTB2RDV0UHpEZXZmM3pMN0hVVnpBQlZrSEt4T0M0ZG1lYnVlN0JpaXZMa3FPdDBKOTFBVmk0eCtUNnd3alBjRVg4TWM3eTZNTEVSbzBqenJpTFFkemtmRXNLWEQ4c1NNVWhVU09rTk83RjdvOW1rRnlRVkpaczFxb0xORWMwYlo0ZmlVT1grUURmTTJ3NWdoT21UYzkzSXU1ZTh3bE5XR0J6V2xySG02NXZQdzVtR3dhSDhSUXlSUWRTaVJYY2dxN2x2b1l1SzJvOEhhMXRFU3U4aklHYUl0eUdzQko2SVYrbVFSTVNZUzluTU5hMEVnWWgwcjBwaTNudTNqREhKSFAwc1poMHpQTDRoZ1FzSllzdWd6YlE2SmZSU2VRcFl2V1FKMUlCOWhxc3YyTndBRGZwcUgyQXh4WWh4QkdNV0UrMnRDSjRrU3dNUFd4SVQ5ak53THJianplMWd5aC9CL3BwbE5SeU9Ea1U0QUFBQUFTVVZPUks1Q1lJST0jJCM5OTYyMmNmNDUwZGE0ZWE3YjgzODVlMDZhZDM0OTc1Mw==\",\"CfFk\":\"ZGF0YTppbWFnZS9wbmc7YmFzZTY0LGlWQk9SdzBLR2dvQUFBQU5TVWhFVWdBQUFHUUFBQUFlQ0FZQUFBRGFXN3Z6QUFBQUFYTlNSMElBcnM0YzZRQUFBQVJuUVUxQkFBQ3hqd3Y4WVFVQUFBQUpjRWhaY3dBQURzTUFBQTdEQWNkdnFHUUFBQWl0U1VSQlZHaEQxWnBiYnh2WEVjZlpEOUNucXVrYlNZbTM1WFY1cDBndEwrSlZ2S29JWUJkRlViVElReG9nOFlQZEZpaGlGRUhjRkVFakozSU0xRWlRUXJGVjI0QmF0eEZTMjRpVHRIQ2Fsd0RKU3lJa2dsK0NGTzBIMERlWTh1eHk5OXhtdVV1cGRwT0hIMVk3Wi9lUW12L09tWm16OUpRNkdod2RIWDBqNllTenFOMkpjVzhGVXFFMTYzeFNqK2pIV3FrSmxVd2IwdkdBTmVhRU1rcWc5dVBpR1kwRzZBQkx1YnNCUlRVbDJlUFpPaVJUU2M3V21rUzU4MFVacDl3OUlHclFweDg3bzY0MDl2OWdjeHhFN1NZOTFaMXdIc3o0S1BEbWxsSDdTY25ramFqSkp3dlNtQk1GemQzRFVCcGxVTHNUNlc1TFA5WlcvTkpZczE3U2ovOVRRU3J0T215LytBOGRiTHlXaXhuSGFsby8vdWFmNzhMR2luTkVsYU5lN3J3M2NZNXFYenFIMmtWcXE0c3YyWk5XSDdYUG83SnVSTFFkaVo3aEIxdEJxcFYxMUU1WTMrQ2ZQaldldElUQU1LOXJqemFoV0tHUlFRUWhtT2VFVEwzSm5lZmk3aHdyVXNqYU95RHFPOTZjYm9sVmFINWFsR05GeUFkYkdqdytyQW84QVhzUGpIRk1GRUpOTWRmUkE3aS82NEdMbDc1bDhkWm4vR2M4Q2diSkZkUXVzcXlNVUR0R09kb0ZUVHQrWHZPRUxDZnhsS055cGZIZzVoT0lFQWcxV2dESXdtekR5NWQ0TVFnN0h4NXduL1VveURTcnFQMDRxUDVWNkJXTDZCaWh0WWxIVGFHU2dXQ0VScXo3Q0xuL0F1NThXMmpFMUwzR01xVUw4dEpwU1F5ZHY5M21QMC9nN2gvL2l0b1hwYmNxVkVPZmZ3UURaUWUrTTJYcDFFZHdNTE5IcWtQOXFLUkQvUFZ6NkNxYnFIMFJVRUdXS3h1QzdYM1lIdkxMMVBiOWFhSkt4UFh4eGpMTkMrSnlScTZqODB6enlFKy9iWW53MHArdTZ6bUVqYUJBbkRvc0ZWd0JkVnpYeFRCaDU0bzFwMFhDM2IvRCtlL3VMc2J3WS9qU25PZWQ5M1F4ZEVHaWozUHp1eVZXTzFtcHorSXVRb1RvRUowc3dWMy9BbnpBakgzMVlZc1RKQmd3bnNER01Nd0pZMkxlWnlmS2wxZjJjYWZQNGRVci83SHVQM2pqbGlYSTRJMS9XM2FXdHYvaEZnRW1yVVRablNCODd1QWRiQWQzejliN2RPeXpNNVlnRjNlMzlBanhkdmxsQVJPR2tHeUdMVkZxUGxXLzlwTnp1TlBuY2VNdS9hdzd6eHBpRU02OVErMGlHMUdqbXlkb3NURWtHa1lKYjFLT3lPVnpRVndlWGVCSmR1cm9BRXV1WEtmT2ZlWWFQRUN1SVFTNXJwbGY1cmIzR1NFUWZqN0dLeDVNR0lJUktZZHc0ekhHMmV4UzVJb3Y0TnhNakNYbFBiZ3p0Zlc4OG82RVc1cWFjMy9raEdPRWVOdkwvTk0rUjVCaW80YmFkZjUxR1hhUTZzckVUWlVsQ2ZPcmZYaU9GZVRjSVhwZkptL2tPa0pyMkxIK0xnWitBZDh6QldFU3VrbW9TWHNac3JUTml5QW4xQUhmM1JmN1JtZHVNdWtZMzlHanh2a0JGRFluekFUSnFJbytscDhzMnVuZWhyZE1ZVjQvT3czMU1uS05USFdWZnMvSXhteEg0S2svVXpHbVBQZURlOVkxM3BxOFBTSEJKdlJudjRCdU1BTjk4NkZpcWkrQ21WOENpbEY5TFVyYTc3eDhiUlk2YnN0ZWR2bWg1ZXh4YUE3OVZKQkxwK0MzczA2OU5KR0ZLWVNib09hTlpjQ3ZHWlZjdG1PczVWcW5EWC81MlhWT2tBdFAwZWdaQkViNjBad3JHNVIzSHRpRTdrLzhDSzY5ZmRQZ3pWMG96T3hMeWl2dzVKc3p1dzNzbkpWK2hUc1hxU2JIcU4zRTQrdlBmNUxxWlpKd0QySHZHU1lmVEt1c25kZnV1a0liOXEyL2pUblpMajBITDc3TGI1MHNBcGZRSDdzTzl3N3NjdzVCdlAvOFQyZ0VUTFptRHQ2NmF0bVdsS3R3WG5BK2hqZ3ZTeU1jUnUxMjZCR1NyTGN0UTdKS3Q5TkRYWm9UdUR3eXlzSmx4dW1FWVhTZE95Y2tFaVhKdHZQYVZmajlaVjRRK1JvZTh2bStOWEhuUUVqb1RxajdjSkVUUjA3bzNCS0c1SlNUTUVnM1VEc2h0VWxGczEyeWF0M1N0R3FpeWZBa3k1WjNZbXdwNU5JKzNjR25ma21UKysvZWxnVXB0NXFTVFp6ejZPQmplSFVSUWRwM3VHZ3BaODVBaG5IK0hXYjVJdmxFK3J3RldRczZWNjhZbkNDMWtKR29DZU1oM1Fid2RZejExN2EzWU9nUDhvTHRFTDYvd1M5M2JITklCQ0hYZWFzZHFEVW13cjMyOVB6UDQ0NjNnVzBHZFZHZTNyTUVZRmxTdHVISzUvTG5FYnBENThTY3RVbjY1UUlmSWRHbzBYdXBQcjdMZDVuVWowRHBrNWNyY2k0eHh6V05MM25YKy9RZHhrYUQzblAyNXFIVXJiUDN1VVhzME0xbXoxOVlSVHQ2Rmw4dXpDVjBrY3lQNzgyV05meitoNEcvRUFRdFZIVXZpTVdEYTNDV2JmaWN0bEVFRVlrZ1lyZU8zVmVjcUtBVzdOOXA4QWw5WDAvbzVwZ3B5RHhSMkE1ZHp5SHNKcU95QjJkbVM5djZRTFVWeDYvS2plQTRTSnZqbm1xLysydkg0b0lRaEwwdDNjbllkUVJzSDB3UUpGVGh0eUVtWHJwVlBTeGl1NjNPSFRvcml0SXpYcDFTNUlSZWpKNkdLNmVvU0tUdjBKYzJnV2FJLzY2RVNEMEdLeTdmcldoTi9JY1pZYTlSempzS1VtbnhyMDh0QkVjYk5KaDlMbm1IMkNvRzJLNTk5eko4eGM0cmtCZ2p5VkZNNkxNT2ZiUEE5eHFzS0t6ZGJzdDlYcFdGaVVPdzVrU29odm1jMkEvTDIvUGVDRjBGa2hQL01TUEVnbCtPbktDUnhIVHJNMEd5UHY1SEQvbTY4ZDQ5WEpLZlNISExuVTNZYTBtMk1yUVJSZWpRaVMwK0lVc05qUnlDM1ZZSkswZ3N1alpYbkhSOHNXckxWcERVTXEyNDdNaHI1djRNRmcwc2RhRk1aZ1M1ZEFZK3RlenVzRXZvZG9paXpOdHlaOGZjOUNLc09Dell0WVZWNnROTTJkaXRKblNpSlFpTWpWN1FrMC95YTNSRVhld0ZmV000LytjMnZ1a1RoTmxQZ3BqUVQzZG9Zelh4cGlIYWtKZlpjVEZoaVdLMzViNWFKUTVqODhzdDJ4SVlvNnJnUC9Zb1Rpcy84VnFsTGplS0pYK01qNUJHa3U2RWZoM1Ewbnk0WDdqdytwUS93Sy9aL0pIYWdlZW45bkpwcEk5SEE3N1pkVEtHSUh2d1E4dmhzdzVkUUtyQWtHdm1rUm8xVUdGTXNIdE1kRUVxdmlHRTArNS9Qa25vWmZCTnRIcFFiQXdwbUpOTXRHd0J0WnRFUTNIVXZpaG1sQkRJZDhySCtOSTBWNUdmWnQyZXRuL1hjZXYyalJQQnp1VTZxY2V5SytnL09JOUNKbS85M2M1WHVURW5lc21TWkJPL2szOWw4VHFmSUlvU20zNjNlQ09zT3lmU2pFb09PdzZsVGhhMUV3S1JtR1F6djVzbFNGMHpJbVRWSy8rbXFLdUZKZWVzNWV5ZjJPVWE3bnh4WHBGUm5MNG1mZGl3Z2hDSDVMTjl5VWtzUG1YSW5aZHFFKzVjbk44TmtRbDlWeDhPa2w5REhzRi9BZmJhRVd3ckxoc2pBQUFBQUVsRlRrU3VRbUNDIyQjNjc3N2RjYjQ1YTM2NDJiZGE5ODI2MTllNzVjOGMzNzg=\",\"KZZS\":\"ZGF0YTppbWFnZS9wbmc7YmFzZTY0LGlWQk9SdzBLR2dvQUFBQU5TVWhFVWdBQUFHUUFBQUFlQ0FZQUFBRGFXN3Z6QUFBQUFYTlNSMElBcnM0YzZRQUFBQVJuUVUxQkFBQ3hqd3Y4WVFVQUFBQUpjRWhaY3dBQURzTUFBQTdEQWNkdnFHUUFBQWpSU1VSQlZHaER6WnJmYjl2V0ZjZjlSK3hsTDdKK1JKSmwvYlIrMExKK1dMSWxVclorWmlqU3ZpeFlCN2RiaXJWRnRnSkpNV1JKbGlaTjNBYkptamExWjJBdWtCb05raVo5U2JzYWFiTU15SUM4Qk9oREdqVG9TOUFBZmZkL2NNckxLOTRmNUNGRjJlblFodzlJM250SmtlZXJlKzQ1aDV6WTJka0JrV3JKSngxanFKRWUycjViT3BHSXJTMVFtNVNPaytXMmRPeEdMcmNQYmQ4dGFuOE9iUmVwOTV0bys3allCTEdTTDh1R01ibnluMjBKYkl4WEFvdUxiUC84eFZzRzVuRWtYbWY3M1VLQzdWdHBCSEpvKzE2WW02a1kyM3pFbTdFYmtTck1SR05vbjBpNjVTendSRGJQWjhUME1qZU1HMVl4TUxEem5NZ2x5a3dJcXlCT0ZPTDBYdi85emsxYm4wakRQNFcyUHl1bUcxRzI3OHZFcGI1UkRLSjVxUHBrY1d3elpNRzNJQjNIdWcxajI1L0gzVUFwU1I4WUU0VXdDS2ZZdnZWY0FqRitXU3Q0RmtLRWlFSG9MTXlnL2VPU3FHdG9PeUdVczd2cGVIWFoxclpYWEYxV0tZK3ZGYVh5YVBjZ2l1S0VLWUlwUkhlMnhjNVA5c3ZHVnNzcHJFMWtya1BkaUNuS3FKbEMwSXFUVUJ5azBMNWZDaVBYa0djSkpvb1Y3RHc3WDhHdjV3dndxN0hKd29mZllkZVRpUmJkWFU4bVdVTGJyVXozdkMwQkluc1N4SzlNbysxT3NCbHg2WFU0OWs5MWZLNXR3Uk55cmU4Mm9ZMGFmQlFuNFF2TFBmMi9hR2I0V2hZckJhUStrWW5PRkhjVDQ5TE8xdEIyayttQ2FteFZMY0xFU0FRR3NQUG9MRzd3a1J5R0QvVlo5UERLUWNUWUhsalpoSWZEZXl2bUp5RWVIaDNPamt1aTVUNTc4ckVsdE4zRW1DSDVRUkR0TkhoOEd6N1cvZ2J2R1Z4bEQrU0lORjduOTFlWUdHek0weTI0aEJwOE5FU1FVVzZPclNzbmprb3o2UzkzNUhHL1JEeTRyQ2R3N3hWdTRGdDNkMkJlY1VvTTViRkV3QXRXTVhTcUdyNVFpOVNDTkhKNmN2OFFGK1RhV1hqdzFOdGFSTVFvTVRFT1FqZzh2ai92SkVjbm1GVlZuL0ZJdXh2NTJhNnRiVHBMbzFsVWtJV0c3SXA2Mm1GdTVITVBwRDdDNGxMSTJENm5IQkhFb09KWng3cXhjZmxmRUc3M2pLM0Iyc3ZDN0hnWi9qRnNyNGJTZkl5T1RaQWJ4NkVnekl3LzNOaUdabDlPY0JOZG5uQmlmSDlrQXo3MTJUazc3MjFCdDZJbVpxWGpkS21nUjRyMnFvaW5SYjFZZm9HN29WZHV3L3JRRU5uRkNqZk02Z1Y0WHhEai9jTnJ2RzlNUXBtdXZuMEwzbVppcVBEMkdqN1d4cm5YSWNmRTJHK0lZUXFGUFp0SWZta0Y3aUVpMkxrTzMzeUxYMk92VE9UOTdnc3pSWFJGbCtEZVkvcHZqaFFWWm9pMUZTN0dld2N1TU5GQ3hYazJ4bzNsZEFuNmZUUGNmQXkzcjNFeGptM2ZaZmNTSHNpSnE4U2RrMnhXRURkbGhyaGtQYW4wTmZkYzVjdHR4UEJ1SElmdnNldnNFYzloN3c5Ymw1akJOLzc2RnZ6dms5T2MxUk5jRE8wNGZMb3U5QTNCcm1rbG1xQzFxdWRmMWJnWVpxanJnRDgyekpZbE1RcG9lTXNXKzZFd3dmendEMkFSWTN2OUtUdW4yZUd1emVyRzduM0pyKzBWTGNhVDZxbWNYK29qZUJhRVJFOW5US1ByYnVzSDF2Y0FiakV4RUxGR2tOSm9BWThoaGNRYTNOWVhjYWtmUXhLRDV4cmhSWHRGd1NyS3pzNVQrR2EvYVdUdWloWUw5c2l6b2Z4V2NtbEVrRlBuUC9PRTlWcFR3Ynl0amVCWmtPS3NKaGlldWkzUy92QWNGME1XU2dZVHc2VHBUdzMzOVR5RGlhSENKNC93YXdYM2NUY2JUU3VTR0dtZlN3Zy9SQkpGbkIxSEhrbmo3SWE5RG1zS0YyVHpOV3YvWjZENUY5aCtJUlZsKytKMVcrR3dzUTJHN0pWMFNaQ281dUtmZFVUakd4SFUzYXRjREYya1A4N1JjRmhUNkx1TGlwNThpZWRqWkdZQ1RKaWJtMXlNWTV1SFdidUllSzZjSU1wWmVMM3JuSUNaUmlLc0h0emtnclN2U24yanFNVG0wZXQ3SWJwWWgzVGNucFFiZ3JRamNyVzAzeTFLeHd4UmdITlhKVmRGK1RPYk9TTHFvQW1EaHIweUdvenplRnpPTjdhZ0djdWlncGpjUE5ya1lqejNvcEd3aXNZS0ZoVHAyQ1FYTEV2SGtpQUVaUXRXOVhieFB0MEkxbmIzWWlwWjVtVjdrMWExTnNZYVlpQ3ZGempjblhsR3l0d1AyZFlOMFlDRU53NzF1Qmk5TjZFK08yTWI0NFI0WFFQWDZPcm5DMitkR0ZPUUhmaU5haFZnV0U2eHVDOGlTanBIWjE2cG9zK1FLWjVRRlJTK2YrcjhSVGpPeEZEaHBaV1l6WWlFbEs4SGlaWmZGcU9VaFRlRU1kaE1NcGt1SmFSajgvY0pQNjVmUjhTd2dvdlRIYlBBU3FqN2FaSTRQMmQvRllBS0VvczRMNHhpK0V2NDRNd2QyUGhveUprUGhMNHo4TTRwMmFnWUp5OXpNWTVkdm9pT01YLzdpOU5ESVFncm14Q0tCc0EzNkVqM1oxSWZadFNtQUpsWk0zQnc1dXNEbUJBeVprZ2M2dHJMSHliSmd2MGJBU2U2TFZybE1KbVlXYUx2Z0RPVk5oU2JjaWcyMTdEWG5IeUY1M25XL3NJbXJKdGlqQ0ZLT3h5SFFjd1B5bXNOTG9aTHZ0RnZWMlF4VG4rRmpoTlpVa2JYcmpCUk9PL0M1NGdnNGt6SkJHZzFlMXlDVGI1R0t3RytzRThsMnM0dUs5T2hvWmtkZTlaTzJpc04rdnJUTG9vOGk1SktsVjVueExyUldhWVJXMFh0Nzc3Y2JuQVE0bDJuWWlpT1Zaei92cm9taWZMNTMrVitjazZnNHp4anhtSHNOWVFndXEyUHQ1NmdZK1ExeGVMYVByb0I1emRNTVVienV4Y3hRM3RFZUFmQ2NNazljTVRrY1FOVzY5T0dFSXZOcENSTU5qTUpJWFdCSFpOelo1QUUwdzBtU0duZy9JbU5sV1Q4Z0ZSc2RFb0dyYUtRM01VVVJUMkJHOTlPSFdxWW9iMkN1RGR4RVJmTEpDRi9IdVlxQldrc0JjL21UVVJSUnRGcWwwR2J0WmRNVE1hZUlmM0p0TDRWdzk4UllTNGlDbWxYL29RWjN5dDJGNGZSbVpQZlorVFN0SFlsMWFTUUdWTE1XY281bGhtVjZsQ0RxbDMzQ0FzVHhBMXlqaUZJdHVkdFdxa3EvMHpHbHJVTDQyeFlSRG1henVMamhuRFh4c0hHN1Jacm1FdHFVdFVnWGxWUVVpZUZzZHRvaFRmbms1OW53ZStIYkYyT3ROTFRHcWg5R2hGR2ExVlVFSUloU0x1UFRWTm44bzIyYkdUZGJTMEY2R2M3c1FVYXRjMGttcEFNcHlEZkcyYm9GbEc4dkE0T3QrZy9zQlY5TmdzbTQ5djdzQzBJd3RoL0gzNGsvV2l5U0YxVkttei84eXE2c2ExdEdNc0Zhb3RNaFdicEtZVkd0WWt3LzlweExKZVY3MUtqRTN6NzhHcWxTQytUaFhwdzlMZkNYdkhGTW1oN3JlLzhGWWNUVG04RWNlak1TQy9odng4TldsemNtSFRuZVZsSkVpUlh3NHRsdlh3VnF0WGRGZEw2VWFmdzJjNWlqVDlZTS9yemZnTGExblFqdUpaTkNPK3lCVHpwYzM3K1Jvc1daYXVOdlg3a3ZRTS9BYk40c0FoR2xPSzhBQUFBQUVsRlRrU3VRbUNDIyQjNGMxYTBlY2ZhZDgwNDc3MGFhMGU1ODZiY2QzYTczOWM=\",\"saGt\":\"ZGF0YTppbWFnZS9wbmc7YmFzZTY0LGlWQk9SdzBLR2dvQUFBQU5TVWhFVWdBQUFHUUFBQUFlQ0FZQUFBRGFXN3Z6QUFBQUFYTlNSMElBcnM0YzZRQUFBQVJuUVUxQkFBQ3hqd3Y4WVFVQUFBQUpjRWhaY3dBQURzTUFBQTdEQWNkdnFHUUFBQWl0U1VSQlZHaER6WnJKaXh6WEhjZDFUTzRHazB0MTkvVGVYZFA3dm0vVit5SWhJeE9SZzAxTWxrdUVzUzJrR0lVazVKRGdSUUlGYkkrUmNTNjJwRk5zUWpSU0hKQVpnaTgrQk93WkdEQTYrS0tETHZNZi9OS3ZYbGZWN3kxVlhUMDloaHcrVlBXdjNxdnArWDNydDd4WGZlN2s1QVRhV2hTcW1SMGc1enpwUmtOcWwrRk45NlgyMHhCc1JzM3p3c2c2UHdzR2thYlVia2NnSnZvbUh5a0xOb05KdTZjZkMrR2NjRzBkdWlEL0R6UUdOYW5kb09IdHdLTXYzeE1vRFJiUzhRWmFXLzZneWZEWEo1RHRlS1hYZm1qVURIM29YQXZTSFZjaDVwbEJzYWN5OWxBaGE1NVgrOVNwa2J6Y1NiRytKdGo4MDRSZ2s5SHpCM1VCdk1PdUlJb00yVDE0YXBPQjFPNUUwODkrMzVTM3pueDJ3OTdiSDRFbkV0S1BoUFN1ejd5bUN4SlVyYWV6V0tiaEpxTWVMd20ydUZvVmJHNFlGcVpTT3liZGlBbTJUanpQZkphSjRRU2U2MFF6UmIvZlRsejhEaktxTGVkSU5ad3ZBNC83d1ZQV1FnM3B4K1ppOHlkcEc3QUl2blNSK1l3WlRXTDZNWlVkU2UvalJEaEtJNzVTcFA4akQzRjJiVElYQkNESXhoTzJFbVF3c0VMTmlXYVh6Y3NILy81SUI5dmNrRjZjdm1rbzFHaDY0Z1d4bytVZDZFZDk3ZzdONzFuTlBuc1l5Snh2UUs0cjJiZ3dCN094SUpxL3FCL0RoWUp3alJDS0JxVjJnMTUrWWdxeWlUQzFrTHpXZU1zemlOZHBONVBUbk5PZ21tSFRIWVlYWkIxNHJzejVCRHdHMDIyMHBIYUNveUNacm5Qbmd4bEZJMUs3RTRZZ29WWnhJM0hja2tsVnBIYVdiK0R1eXorQ1JlWEhNSy84Qk80ZXNkZGxZamlCNTJJaXJZelVibENmMDFwMXJwUVFDN1VkMllyVlVXMURKc211YlhLdG5Dbkl0c0pFKzg1Rk9GR0lRRmhWa08wZjhFN0ZFT1EzOEJpTjVjRlB2MHdNbm40a2I1N0w3aWREajVEeE1LVi9HQ29lWVlBZDBZVDl3c2lPWmtGOFlpc2xxOWdUSVRvWm55Q01WbklmcVF6UDdzREZ3eVFFQlc3QXZqSG02RFpjTVFSNStUWWNvL2xZZ0lWYWhsd3haMzQyLzhhU0ZNb2tXQkFlUE1lT2M5NWdXN3p3N1gzNGczSUpmdUdSb055Q1ZwWE4xYVhrMmE2a0NWZ1VMSTRka1hnYm9zRlZ3VndLRVpFS0laTDg1WE82R0lRckgzOERpL291N1AzK0xiaTVjcnhuV1JObEltRENPU3ZpZTB2aHlQRmZuLzVKeDdDN3hhd2hIaC90UXI2NktoR0JSM2tEUHZ0V3ZObW1CREl0ME9vNGZiRDhkZS9QakNBQlg1VVJKcWpRdWVWV0hIYUN5Wlg5RVZ5VE9GN2czdk9tRUR6em4xNkZicW5uS01JNmVFRkNYbXZIWURkbjMzd3dSZjI3OTkrUUN5RERQNFBoMERsZkI3dHNXN3dUOU1QSjB6MjQ5REFFMFlkaG5VdEhoOHdZUWpPWTBNV1lCR2gvbjI3WEdXRU0rSG1FL1NlYzQ1ODhrbzdMWjJtYWt2SE9GK0w0VFRodGRCQ1FJQWR3RWFXcER4NmN3Q3hLMjBsdFRHckxNWHgySGdseS9qNThaODVsYVF3bjBGRnhhN2NQdjBVaVNEbllnOFBsMktsQzB3NFJoR0RkdzhKZUhDNDZPREdNMUpQVDFOWDVWWmlaQmYxNXlPUGFzZ1Z1QkJrWDVBdFJTNUFIdHl4bk96aDhFSlJ2RVdRaVEvM1lINFpBS2FJOUt5NGluT0NqaFFoU3pnVkFzZW51ZUZIbVAydWdJbjRaOXA1WklqVHJOZk9jb044REYvU3JRYW1JbnJCOHZWV05zRHU1QTgzS0ZtY1RJYndnSENSaThNUjF6TXJrQ1RpRUR3K2MwNU4yRFluMTlUNXpEVWVKMHBNMEh5dE1VZjV6MllxT3BTRFhrUUNtQ0N2NjR4a2NmNnlaYWVyU3JlaHFYbG9YRW8vZEJDY3hFdGtSTkFQT2kxZVVzcmlVWk1jeWNoSzVOSE1Ud3RRbldZUTl1VzQ1ZTVXU2pHdTd5a1Fjc3hLazBLWlJhQWp5N3AwSEFybCtUN1IvK2lZTWtTRHZyWVF5L2liUDR6OWFkYVNyemsxN3ZTbHUwWVRpdStaNUlHKy9HNkVxZFVZUXp3N05IRTdFUExUMXJ5MXJNbFBVTnhIRnJuNWdEcDlhRVlFZEYxUzcxdWRQWHJJRStmdE5aaHpCU1JSTWFWN1hCYWxLQkRFd3ZvdFBUNjk0aGE0SksvVGIvM3pmRmp5T1o1dDBSZEFGU1hrVHNJandvWFFBSDlpdVJYNEZiNzR0ZDB5aEZwVGEzNzF6RXk2c3FTVVhQbUhua085aENNSitOenVPWU8vWUVDUUoxNTVTdXlGSU5PMDN6dytmL1EydDBJZndPNG5qN1JEL0xvVUlFYXc1YjhPdmc0c1F5amhLd3l5MTdBUTYranNRTG5LUUlKcldacHpJazd5V2tEcGY1QVg0Y09WQURCRmpsZzRJZGhuVmFwcHBleTkrZjJSZDY1Wk5NWFErTDhPSUwraVllM1NCeDcrUWN5SVJyUXJSTVp2WmIyaktXQXJDTy9zV2ZDVU1YTi95RnRJMEJ6YzYxcGJLdzYvNWlMQ2NIb3FRMVMxdWg2L0R3OVU4R1NXMDIrdkoweTJlWHBMdHZ2UW4rSXRmUTgxMDdJdndHdmQwRTRnZ2hieTFRaCtaQlIxanRjQXBqYmJ3Z2JCOVFVNTNFMXVucS9Ta1FDTkVXSjNqbGJoa0crVkZkZjNPN3VIUkM1WVFYRUUzd1MyeDNSZ0pobU1INHd6amFNcGY0T2YvdFJ4NzRURzE0L21KUll3cDZPMTdXQWdMSStXNVlSTXhLajU3LzYxU2xrTzk0SERYL3VKMlY1NktDTXFyV1V1MFZZZlZudEtuc0RUd1FibzY0cHh0TWZiSEJadDU3NmMza0dOdlFEdkNiODlZQloxQTlyQkkxSHgrL3kyMmRVWnJrbkUwak9hTHFNMmlJSWltaE1FVDIrd1Z0MWxESWpYTnVjTkNxYXc4ZG80US8wc0p0QmkwUzBYczZ2M2lBOWE1aEVaZEVXd0U4VjRpekJiS1hYRmhpU01FQzhPczlvL3Z1STdhYmRPVmdTN0lORW5WVDh5dGJlU3lyd1R4S2R0RFQzMGV5SFh0MzNhWkxGTlJFbmRVeTZjZk8vVDFmWFJOcHdtdlNBU1IzdHMxOG0yVWFYRDF4S0pWdWdIWncxTE9WNW5WZnZWeUI5MVRqaUhHbVFteUxjclFLcmlHTTBXbk8rRmMwRThQdi9QcnZGY1ZhbmFGbldMY3FVVVc4amVRdkJndEQzMi81SmJCMk9yRVhBdlNiOUhWWkhrMk0yMDdmdkVYZ0lZZ3M4b092R0lueXJLQUY2K296R2UzcVdGejVOdnhocU1YMldWOXNYMlJ0Y1JGMnVJRjZmcFd1eEF1YWZXdFg2MnNGU1RyUGJ1Zmg5cXhxTWw3L2VtYTE3R2JjUGc5M3VkYXcxS0VmWE84dW5admk0aFJpTG4veVMyUE4wRWJtV2l6VEFVWk5TY3c2VVJCU2RCZmxKeUdWR2V6cDJJZHNaVDladUk2UERIN3hWejBOdWQ4SGpNaXJNakNhUXNUaUZ0dlN1ZUR6WDR2Yk1lNWRzVjQwM2IyTER6eWpUVnZ4TG1GUEFzeUtzMzN0YVQ4NTZLbGxQaC9KNmUwcWZHUDNQMjhkVnRLZWI3N080SC9BUU5hSTdGM0ZWeEVBQUFBQUVsRlRrU3VRbUNDIyQjZjU2NzFjZDkwZjIxNGY0ZmJiYzAyZWMzNWI1NjA2NWM=\",\"F2Hx\":\"ZGF0YTppbWFnZS9wbmc7YmFzZTY0LGlWQk9SdzBLR2dvQUFBQU5TVWhFVWdBQUFHUUFBQUFlQ0FZQUFBRGFXN3Z6QUFBQUFYTlNSMElBcnM0YzZRQUFBQVJuUVUxQkFBQ3hqd3Y4WVFVQUFBQUpjRWhaY3dBQURzTUFBQTdEQWNkdnFHUUFBQWp2U1VSQlZHaER4WnJMYjl6R0djQjl6NjJIWG9xc3lWM3RRL3ZlbGJRcjd2c2hybGE3SzhsSmdRQzFtd0F0bXNCRlVpZ0ZFc2R0WUNGT0U4QkYzUlJWZ3JyeUs0RmhOSzJpeEs0dE9JWWRHQzBpQktnUGRvSWdPdGd3ZEFnTUdEN3RmL0NGUSs2UU04T1BYSzRrbzRjZlNNNE11ZEw4T0k5dmh2djYvVDVRZWwyZmViNWIxTVFjZDkyZG5lR3U2OEVBZDgzUzh0ZHRhUUcxYkV2emlqK1NSdE5aRnJMamFEcWhJdTFIMDZ0akplNDZFUTl4MTJPZENlNmFFazZvK2pHVHp0cnlPQ0dVS2NWZUlaVDA1Q1Nhdmh1aWxUYWEvaVJRL0h2Lzl6YzdNcHErRTFBaExLV2FzNXl3WkxXbytRbnI3WWgzSm1FKzBUT3Y5NXBFYlF3YWFRWE4yd3RTTTR0b091SENPMWROWm50bDh4d3J1eFAyUlV2VCtrbW0wclJsdHJwVlc5cGVvalJyc0xEbzNGWDhQMkVyZmhpTEtiNTczZzIyRmpLZmNHNFJYbEJLZmpUZGpjazU1L0ZrRkpTRzg1dU5nVld1U0tMUTRLN0ZaNFRMTFZzYUpWRHBvT2tpWjg3L0FlWjhkZjNJQ1RtNytiRU9teWFTa1RObytsNlNUblgxWTA0MldpOUJTZGtIUUl4a3VHaExZeXZVSytJeldMcklZTXlTYnFYUWRCRWlnRVdXWkVzSWxVRlJZd1VvTDhhZ2tzdlpIdlFrNlV4WDBIUXZZQlhyQnIydm5UVm1QVjZSZk1ic1NXN0hiWGtpdVlKOXdpS0tJTkE4ZEZBblFocHFWRDh1Rk1LbUpLd3N5OFNVMVVUelNSbE8vT2Z2NEt2ekZYenYvSFg0V2ZaVG51VjdYQmszaW1vSnJkeWlFa0hUQ2VJei9LbTluUkJFa3NOYmhKc0VscUd6ck1WeXloVENncFdsRUJFaXg5OWVzNHZndUE2ZmJQSFB3U3JYaWZjUFhJTFgwZ015TitGejRWa2NONzZ5eW1xY1BQL1FWcVpiSDMwc3BNamhzSG51VlFURlZjajJneGE4KzcrblJ1UnB1TXM5NXlIODlnQW1BT0daZit1Vkc1WW1iUlZPa1dNeDV0a1d0NWR4SVZMY0dJODR0cjZCa3htbS9QSjlleG1OWUx3QStjWWdpRXZpd1dVaGdRL2NiaUlTSVd0c0ZIRVZvaHo1TVZMaHcxamloSHk1Yks5d2pwY3ZNVkl1d1orWVBQWnZHY2FEOHpldEN0YTRjTVBLUzZlRnQ5MmpFQmExSkVFdHhrZmlJbTRTbk1qT1Z5QTNZUWt5aGFnaCs3TEorcHFLVlBnd3N2QlhyVXV6Vlhiek1weGlLdHZrNkdYNGhTbmtIL0E3b2FzVC95WW5SQ0drRzJySGsyalpmdjgrWEdDRkhQb0dIbWpwK3h2ZVpuSWlPeEZCNkJYdHJYMWZQSUxQM2RkWFYyRGh1UkNjcEJYOTMwT3dMWlFSSzVjZlovNEZyNW9WL1JtOGRmUXFTQ0VrZ0xxNU9TaWprWG9Mamd0Q0NPM0ZFRlFEUlZPUUVrSGUxQ0hqUWxKbGcxeEJTT1lyV0wxeUM5WUdzUGRoVkJZTGtJMTBkeXpDRGJUTEtpcjBUZG1BOTgwM1g0VVBOVWxFRkMwbkNxSHdZajZHeXJ5cUgrbDlMR3lYZGdRWlhFMHAyb3REejFuTXNrZzNKRmVOZ05NZnNWWWhZakpaS0h3SW54OWl5bXBDYm10NVhvV0lFbUw1TWtRbnZBZTNZMm9OVFNjNGppSEtRZ3pDc3orQmRVUUlCYnRQaElob0JnMGhMSkpQaTIrMjdzSVIyanF5bXpBWGRPcGlMREFwOGRrZ25GaS9PTks0UUNjQmh6TTN0R253RndQV1lPbERVY28ydkdIbUU2N0NTKy90VFdzZzFMU1d6MTY3RHVyOS9yZHc2MnNxcEFXM0hodGQyYWhTS0x3VXRrdjdGRlp1NHZlNFFXU29yZVpBekVmd0ppdGtCSjVuSy96WU50TlNIc0ZMTFN1djlNcWZVUkZUVThoTXpvRzg1TDdTNFNyRWwxUVpJVS9CK3ZkV0hwRVJLWmRkeFRUaStQN0syVXRyOENJajQ4VjMvcWxMd3NvNkVjMko0OGo5SFF0NUxYTU5LcWFVTy9DdUpvTlVmTEh5aVNuRGYvQ1U4SHQ5Q00vSFlLYStzK1g4OGd5K1pqaWtoZlRoN2hZdWhDRDFWRk9JbXhnV01WSi85YlRWYXJMWm9ubk8zaE9JRGwvYU9MdDVHdjdJQm9jajhwdURWTWlnSlN5dk1USzJJRisycjVHeFZHZnNxK1U3WWFpUVVZUERrMWVPTzRpNUJ5dU1DREptZkNtVW9USkUyRElpMWJDa2xmbWJ6aysxT0FhcmJDKzgrYXVMVUtJQ1dEUVozeUcvUzVFamVLQktLTmNrVUdOVGFCNGxHTXBDVGh1djZmVVRFeUlYTXFZWTIvclYxSktlUHQxeVhpNVArZUtjbEZSOTFzeEwxdTFMM3FsdVI0ald6NW5ST2hXMjJLdWI1NUhBNytFbzI4WDkraU40K2JBbzVCUmNGMzZIcFNXVlFlcU12dHM1TjlHQVdKcmYvcVhvUXZ4MTUybFkvL3NscThLM052QXlETnVQdjlXUFJoZDJCcDVoUlNEclZjTmdwYkJnWmQyaWRaWjZmaHJPL09VMFArWm9RazZjdXN6SXVBYitmTnVjeVZHUi9wbThlVTdBbnMvU0xDYk04MGJNV3VOeXd0WkM4blYrZjdqNSt0T1drSzgvMElQRGF0bkRMaDhiOEJFT25uSG95bkI4Mmo4dXByRkMxTEI5RlJxTDFta2VqUmtzVm9Sby9UWXNjYTNqQ3dnb0wwQWhOR0ZLb2ZockZlMTMzYVUwS2pzYjdFMGhmdFZlQVRxUFA0QlZLbVREQjgrMUZPak95ZUFMQi9IeUd0ejYxYUJWR0MyR0gveWxnaFYzVktlY0Y5d3dsSHJJSmdTTDF1MGk2TFNWamRZM29DN0lNTGhqZGxsVVJqS1dnbW9oeFFreWYzOFBHRHFHaEdKQkpqamtGdzR4T0JrL3Z3djNoSHhNakZlQ1kyTzJ0SVVxczlvcVJ1dUhWaEFSRkJxdFg0RURqSVEzdEc3dStqSHIrdGx6ajh4Ny9HbGp2S0FpNWpJSi9aaUp4c3cwZ204dVl0N2pSTE9GdHlCWElhMktwQjAzT0NHL1pMNDBFVUUzbjRad1FCMUh4YWhoYi92ajB6SXpIZFdFdklJSVljdFRzcjJ1UGdsZ2c4TG1lNFBLMzlxQ1o4MTBxNVU0d2NyQXdPNXhnaE15VWVKMzl5cGpwUExaYVAxSGVyUk84NVdRTWFWcjlzanl0aml0OWNKRldQSFFZaWFiK0RwUmNQQTlWQ3NSR2JTQ0ZYaWJFMktzNG9yM0VhYmxESnlkdDJUNFUreW0xaU5ZWmVJU3RwVlFDcko5bGhTYU5ENTlJaEpxeVlvcFJHMTI5R08rRngwcWFHaVhwWXdISEtOMUZuN2M4TWhnNjVhSWlQZU1hYklvSmhtSm11Y1lvWXdSa0ZFaDRpb3VXVFFVN3lGOGQyNlRrWEVORG12bHVWblpqVHRNUHQ5S1pwVFJna0FxaG9LVm9Rd1ZRbkNMMWtVQ1B2eVR6OFlzL2xrbEN5dEVGT09GUXJrTmdZN3paTU9BZi90SlpRZksySmVId3FMaXNXMkl5anZiTHlIRTZzNGZEdWFZM1VoZFNDcm4vaTBXR3h5dVBqRGlETUwrSXI4VGx4amYzU2VoNDEzaks0N2RpaUZNcHZndDEzakUrd0lnb1pJclEyVFcreGN3alhvV2xKcnpOMW9zWFo5OU83Z2pHYi9scVlVUTJ1VUd6QzFhRzB3em5UQ0VwUVpYWmxUYTJoUVNTNmVJUXNiYnhqUlpianBNMFYxUVZlTzNrckx4MGZkc2xmL3lwSklkL2tIMlhpRkYrWHFMMU1Nd0V5VTlTeDkrQUtCL1RGTXJpck9RQUFBQUFFbEZUa1N1UW1DQyMkI2MxMjRkMThiMDA0MDRkYzJhYTJiNzRlYzU3OGNhYTZh\",\"m5BS\":\"ZGF0YTppbWFnZS9wbmc7YmFzZTY0LGlWQk9SdzBLR2dvQUFBQU5TVWhFVWdBQUFHUUFBQUFlQ0FZQUFBRGFXN3Z6QUFBQUFYTlNSMElBcnM0YzZRQUFBQVJuUVUxQkFBQ3hqd3Y4WVFVQUFBQUpjRWhaY3dBQURzTUFBQTdEQWNkdnFHUUFBQWhsU1VSQlZHaER6WnBiYnh2WEVjZnpOWW9DNVBJaVhrU0s0cElVN3hSdklrV0tYQzBRd0FHaWx3aEZqS0FwK2xCVUNWSzdhbEZiRHBCQWdZeldqU0FrVVJJMUJsd0RzUk9nQ0FVM0Rud0orcUFISXlnc0IzNHdZci81MGQ5Z3VoZnU3cm5NTG5jVk9zM0REK1RPSEs2bzgrZk1uRGxuWDNqMjdCbGdCT1FlYWxmeHgxYXBhem1SbzY0Tllxa09hbmNpRTZtaWRwSkVWVUx0YmhDa0tHZXJGdTMvVjRONGZBVzFlNlhXeUtOMkExdEJwa0UyS211dnFYeFRlKzFWUk9pczhGOG9NZ3h3Tmp0S2ZmM3p3MlNOODAyTFdpdU0ybjhLbnFzZ3p4c3hrVUh0S3FtbWZSUzFjL2FmU3dkOXFQMm5ZaXFDNUJ0NG1wa1IrNmpkamx0M0wydGd2dVF3anRxbmpTUW1VTHRYeW5WNzBkdFpQUUlqMFFIbm00b2dRcVlBdWZnQzZuT0xJUVlKT3lZK044ZlovdC84ODh0dk5OVDNSV0hSdEljWHJPK2FhK2dwMncxVFMxbWhkQU8xZTJXU0tDVEZVZzFFeWZvbEp1SjRwRWFIQmRTK3VtcWZucjQ3ZUFuZTJlQzVka1NQSXdXSkR1Mmp3aTIyZ2hRRlBrV1VZL2JGZDFEM28zYVRwN3Z3NnRlL2hMb2I3bzFjQzFPUUs5cnJVcXpNK2J6ekVHNXQ0MEt3R01LUWdoajQwaEpFQWptUU15WEt6aExvV3hIbHorZy9EbGNSSWl5MU9GdW5xdi9xZkFIN1ZCVVU5RldXeGc4YitPUWpYUGpCdW9lVE1QRml5bnpma3ZRbHRwVHVRQ0t5Wk5veDVEYjJTNzRMMTVDSnQrY051UFVJRjRSRlhPckQvdFpkN2YxOE5VdjV5aVU2blUwdFpXRjBSQ3NsM0x5SFR6NlBCQWRQK1h0eHdueDFBS0ZmYkxoajQ0aTdIdzBUR2R0WDRURTI3dWdpSVlqQ3UvdW1HR1VCN3kraTVZZ21oaUhJSktZaVNEMWcvVkp4Um5EQm1QQnZkK0Y3ZEl3aTRLeHo4MVZMclZpaS9QMENQdmxPMkFsRFR2U0I4OFE5SHIxaGpmM3pEbnlHUkVldk9VTmRHNEs0RWNVVXBGeVo1NXh1U2N6cmEvNTgyNXBRVVNDS1BGRS9YbjF3MzdML0NHaEJObUh2UHUyLzlwK1J3dWV3TnlRRVVYaHg5NUU1WnVEVDB3Vlp3Tm1pcmJJOEVLenJSMWZoUTBhUTBaWDNUWCs5dFd5TkhSUHZwalV4R29YSkRiQXB5S0k4bmZVM1J1OTh5RXhKWkgyWVJDaVlwSzRGdVc1ZFV5bHJFelkvdnd4eWZ0YjA2NEtNdFBlSEc2UW9CM0JvM0VPRFRsZXFJS1djbFdweldYNXhvd3FnNGxRL2ZNa1lkZTA1UXR6aWk5S1RSQkxJVzE5K0xxaEhYREZkZ29OdngrbnE2dzI0T2ZhVHlNVlZrTHBkenU0SUlnaFpZd3d4K0xIYlVQR1JlMi84eXVyRDBVTm9GdlhWVzdsRkw1a05NUXhCdXN2MHhIdWxGcUlYUmE0RldRcmpmM2hPV1VGZ2RndWlmdHpiSmNRaCtQZFo2S2RtWUdkMG0wSllhc0pDUWVUc0d0czd4Q1NmaC9XRDJ4Q3FWU2xSVlBpeFZvUlVvdU1Vd2hackNuMDFwWTVqeGJDTERwWGFvZ1NaRmU5Wmh4T0UrOGNWcXJGWjZqbzRFNk91SFRsYzV3VkFXWWMzbGZHSmNCaS9Ed3NpaU9GalJibTBib3hUR0k3Z21QaC8vYkV3dEFZSnVsamI4aXY0OUJPOVhyQ0NMR2FkKzZEWkhsR0hGRG9GdkdaemdyVG0wdVkvVnBBQzV2dVQ4dVlOZHVLYjhQcWg0c09FVWhwQzl2dXdMT2JIK3o5TUdpS0x1bEUvT0RHWTR0OUo4RnN4ZGgwNmlaclNKa1VJUzBFbWVqSUhYS1dzVkxnQ0Q5ZGljR2Qrek5vSDhQVEJCM0JvWEJ0c0hpck4yamcvSy82aitSRDh6cGpzSzBHNHJuNk91VGZkbjFnMUpqR0hiM2VZZU9sRE5DN0FwWEhFb1BkREdGMDVCM3VJSUNxWDluVkJLbm44TE1pZjgzWitJaWYxOUtrSlVncnhTeldhWTBxUW83VlR0QkFrbVF6czdQMkdGOHZrRkR4OFFOeWIybElKb1UyaFFVTW02cGhuUVZRc1VTWUpvOWFKWGp5aHZXbzJoNmF3VktmVFQwT2swOU1rOHJMVlZMb3M2c2V3aVV6dWYyK00vVm8wOFA0N3IyeUNLQWRnNitvbVhDZnRWS1FRUmQrbVMwZEJVdFp5SzZPbEttR1oySTY0UDRJWEtWRzJ0UlZaTDFPMStvbU5pL0FkY1creWVKdjNVYURxakNJSTZiTmp0dW04amNOaUs0alVKUTk0anVGUEM4U0VLcGhpS016VXV2QmtrL2FyNlV2MWJWMzlXR05ubzBuNG0vRFpubTdmdXZZYURFMUI4R1d4d1ZLM0NzUHlISFRDeXVyRnBvWVk5WVArN0JHOFJZcWlkdXhLZy9jM1U1RGZ3M1VsWWhicUs1b0k4VkFZcW0zK3FKY1VaT2ZTZGM2ZlRGc25qWUcrODNJNE8vREJvSzd2VHVjN3E3QXcxSmZacmlPRXF5SE1tS2Nma1duc0RQejZyRzg4eVFwcXNiNXhodkJiZ3Z6aFh5VnIzRmRuTlZ1d0dLTHVqWUlJZ291aHd6ZUg1R2JpYWJnOHdsT1pMMnpzT0pEOXl1dXdQbkJYSStTQXQvN0tVUkIvMkNpc3ZDRERDTDNkemdweTlFQnlFTVNJc1B0VVgvTHlGK09vR1NNcXpTVjVUZjQ5cjRJYzcyNHo0OW1HOExkd3FsSXpSU0dGQ2RRajlPcUxxQjhxbFFKL0RyUGF0RCtYRHhib3ZTNlNrMFhJZkhKaWhGQ0NxQnVLaUNEVUNvdFo4cEpDb0p6ZllpYllhNFF3TllIazRudWFJS1A5MDRqL0l1dzdMSGtYNTYzZGlvem8vSVNKU3JGbTFidEt1UFQ4VXRZVDlrRHFyM0hDSDRLM1NKOUQ3ZWdIMmlBVWd0cDdlMEgrQ0d1N0gwTTRLR28rNDdQbGpMRzN4ZFFRcFRuOHg3aG9YM21iblhBSHh0dnlQWUUrQzAvV1VqQ2MweCtOOG5mdEcwUi94RDR5REd3RkVhaEdoaGRrclVZZlduR0NLRFpYWnlBTzIvR09VQ2xMRjRRU3pJQVNUdWVWdnpDcktNZXRFNVZUV28zaHZvTUQ5ZURKSGlWeUZTRUx0YmpIQ0VscWdxajI3OG5VeGNEdS9CWUNlSk9GUWduaW52NTVYU2h5U2R1dW5leDVnTGhQUk8wL0JsZUNwS3B4T0pmMElvZ2VJYVFmSytxaTFLYkhFUGc2Q2VqN2hxaFB3Nk1naGhDenc1WVpQZWg5YlFpbUo2K1dCbjI5UVF6MjZFYTdWODlDdCtIdVlVQlVrSldhZGZpdWM0SWFvdGhrZ1dpSzBGV1doUnlLZ0ZqWHo4WExrdmR3VjR0NUpvZHYyQm5waVVwVEJFTFdlZ3F5blpuTzB6TVlLdzdQSHhpWWdzeU9IMlR3KzdGZkpTNkkxTENhSjY4UnN0dm1uOGdJUnd0UWJhVmh5ZStEeHZMSm52TXFyTkRiRms1Qy9CeHhsYkt5aFJBdHlFSjVLaW1MOGsrQnNNUnZTRDR2TVJvRmIvdFZLdUY2R3JWYlBJUC9BYWhvM3N5ZzNvS2hBQUFBQUVsRlRrU3VRbUNDIyQjY2MxNTZjYzNlMzdkNDRmMDg1MmNiYjc3ZGI2ODdiNTg=\",\"QxRJ\":\"ZGF0YTppbWFnZS9wbmc7YmFzZTY0LGlWQk9SdzBLR2dvQUFBQU5TVWhFVWdBQUFHUUFBQUFlQ0FZQUFBRGFXN3Z6QUFBQUFYTlNSMElBcnM0YzZRQUFBQVJuUVUxQkFBQ3hqd3Y4WVFVQUFBQUpjRWhaY3dBQURzTUFBQTdEQWNkdnFHUUFBQWpjU1VSQlZHaER0WnBiYnh2SEZjZjlEZnpjRi9FaWt1Sk40azI4VTh2TFVyeUxhdEVDUnVwV0NOSTBjaFBEMEl1UUpvWUJveFZzMktodFFFR3NQS2hXWTZ0eG9CU3dVTmhRYmNSQmdBQkpBeHZ3UXdBbEJtTDRKWDNYTnpqZDVYSjI1c3pNY3BlVTgvQURkODdNTHNuem4zUG10aWVPam82QVVLMld6R3VldURvdDJPcGR0MkRqQ2J1VFV2c282cldLMUU3SVI4SVFuZkxBakRvdnJUOE9UWmZYdkU2TjhmeTVVa3BxSDVlQklMNjhSMW9wSTZZdVNlMnZnb09iTjZSMmxwQy9LN1d6S0Y3N2prTDR5NTM3aUxhNkxHMzNjMUZyOVZFWlJRZ2hrSE11MEtURTFBSXE2MkxNTDNiQXZ4eEZkanZDZGFObnBpUE9PZ292QUkvc25sZEJMaUJtR0JsSWtIakJoU3JqRFdjUDBVbm1hYzlkWENpaU9wMUNaMDZ3OGVpaUVHVDFoSkRmTDdYTElJNXVWRjNJOFFTOVRXa21pKzVackZaUk9aS3hUdVYyZUlvaHFkMEthWVJZMGN2MkJGdmJVeFpzUEJGUFRHcVhvWXVSTGZsdFJTRTAraDNCSm5NOGdXL0wwOC9scFBhcHFyTk9rQ3VMdjJjY3hoSkV6VlZoS29vVjd5cSt3V2M5M2tCMlFxNHAvNE5XQkVxTGppS2xHMCtiMXpMSHM3RDM2WlQ5WXNlYWhFNGxDZkdsMXVEYVY4Tmp3YVNNSmNoeGNHY1hwSGFkc2pjbzJFYUpJbk02Qzk5ZVJ0U0YweFJQTE4yVzJnbUJSRjFxSHhjMTJZZUZWTk1zSTBGY0pYbHFhYW1pTXoyaFdWVE9UUm1EYXFJVVFIYWRaaU1qMkt6STVJeUkwMkZGNlpWbWtkUFgzN29Ba1pOckRyZ0MyOTlwWTF3VGp3dFdMSVFrRTVybmwyQjlUelc1OXV5SGdiMm1LR0piamtBNkx0anlmUVcydno0WXdOY0pFYkxvbjBMbHpZOGZvcklWYVRlZHYvT2s1dTJucWp6OVNtWGdlQ0pJc3NJS3NnZmJMWm56YldnOWdFUEpkOW5DQ2ZMMnVjbkdDU0lDUzUyYk9ObW1MRjBRcVNndmQyQnpJd08zZWZZK0Y5czZnRHBiaElneUhSK21rZThld0NtWnd4MXc2c01Yd25mYjRUOWRSb0tRQ0JsRmNIYkd2SllKd2JabG1VQ1F6K0d4VEFpQkZYankwcmhucml5Zk5zcWNUK0RiRWxGMFBLRS9VaWV2ZlN1MHpia1N0S3lKRjJVRWlmemlWMmFVTEZmdEk5ZVQxUmFaRmlsTFJqQXNqazI5WnN4V0NNS0o4cnc2dUloTTRWUkYwTVZRWjhnaXpxa1lERnJFVEllTW5rMGQvaTlZL2ZNRjZKeWpyR3hTRVhvZXZYZTloRTh1TTIwdVA0SzdRMEZ1dEttRHp6L0F2MWZHNFlkWHFDQmpwSzM5di8vYjRPT3pTSkJ6NzZ1MFRxT2NTMEF6dFloczA1MENLanRCLzA0aFFwSWhPbERwWWxRU2REcjMvWjdvYk4zZWRuRUQrYk9MdU4zR0tseGplai9oNk1kSGNOWVVaUk0rK1pGNWhsWm5pcUhWbmIrM0QvOThwTE1OcDRselQxNkd2dzVzKzlJL2FQTHVaU3BJOTVaUVgvSFVvQkpLQ1BhbzJqV3VPVUV1N3Q1RzdTSVZEeXBia2NqWHBIYUMvcjlIcGl5Y3Jyam9PRVBURVAvZytaRHhSejc3RzlOKzR5MzRqR3MzNE9aSDhEdlQ4WmRNeDI5dFhhS0N2TkVaaXFGeGpYSHVhOXVtWFg5V0lhbUl6OWU0MmgyMjEyaTk4YW0walJXRC96aEd5dEp4MXd2UTAzd2dxeU4wMnZLMUVCSWtrTUdMUGlTSU5vanZtODZsNHdOQjltZDBrQ2pYMzRkZDRsaVdlMXVNS0RmZzZxTlBvZkVtS1YrQXRldjBlYXh6Mnl0WHpYR0YvSTU1RDVkNnRURWtNV3dmT2JrREQ5azZHMHB6dzJXQWhTQlRHZnZ0b0ZHMGZlSU9zV1dFRURGTVFWQWF1Z2pmTTIwRkIydWttOUhoOVE3c1hxZWkzUG5IRG1xbm96K2pXRDNOcEMrV3UvQ2wrVjB2bU9udTZtQjlNUmprazE0a0NnR05IUnJzZUJOUmpIVlRLMnE5WURVWk0wSW1KZGlOWTBHeVdib29xbWJ5VkF5TlhMWENSSWk5SUN3NHV2QzlMS2xmL2w0VTVkWlQydVlZMDEwbmc3OGxFd2lTcjRqYitETnVmTDZ5dENCMkJ0c0lJZVZpTnNNNHRRQi9hRmlmR3lTS0NuU204VlNYblJEc1B6NUVkUWcwMEsramdaN3Y4WTdnWmxYRjJ1Z1ZleXd2MmVGMklJakg1U0RTSEhEQ0c1Q2ZJL0NDSEIwZHdwTXQ2dFRIejNCN1c5aVV0N1VEUHczdHZoVGVsSHgrZnhOSGlEbjdZdE9WUXlaZG1YTk12NE1YaG45YUc3MWwwbGlTNzRNRlUzU3hhSVVaSWY0ZzNjY1N4VEJ3M01zWnNrWHlYSGFXSms0S0RMaTFoNGsyanFCMDlicmw0THk3c281RU9WYXFJa2dpcEo5emRpcVo5K0JkNE5MVTZJbUFOR1hKQkpsVGNvN0dncERQK2h6NnpOdmszZ3g4OEJvOVgwZ21oMXZwYkxyU0ZvTFBqNTVTVVg2elFSMnRyYzVUQy9LRnJBNjdjT1JYOG1xUzN6ejhRbHRQVVdldkgrekNDMVN2TWVZWVVnaFovelk3TEFWSlZHVTd2L0swcFZSb2IxbktHU3QvbGxiT3lNcy9QVjR4NzVYdGVYMTVhK2g4amJQM1h4cDJUYVM2VnE1a3FaTkg5WHFQMHNGalRleWlUZHI2QVE0T0dFSDJMc0VUcnMyTFo2dE12UXEzbitQNlY4VnlOU1VLWXBXdVRFWk1mM1VxdFRJVVMvS0JjNVFnOWRDdjRTcUpCbUhWZmdmcXhNRW5OK0FtYzUrVUJ6dElrRjNKbEpqbHlWZXNJQnBmZmNIVWN4R2tRUVM1OTgxL0lMUVFHbndTUW9rc0toUFV1UnpFYS9hbmppTUY4V1dNUXhobG1GSVNYV01BUmxzb21tT1ZoSGlHNEZiaVpvb2prYlI2QnQrSDd2bnZYVE02OE5wRGczVnc5ajJodnRjMmpwRTd5UzRFWjcyMmduaFN4bEZCb1dEOFB6NENSa01qU09aNG5XNG5KZGlXM1ladzVEZFlNWGFFeEh2NlRBR25MdGE1YWpROHZENkVkOTVqMm5EZ1NjRlRKam8wbUxWSHlsZUVoMnRENTJva0d1Zk5kbVphMHlnRTZXd3htZjR0RldURTZyeWZKZWxWakFJcjlQR2p1OFRzSmcrWkRWZWczRzVBdUZXQmRHNEtZakh4M2JLRnZ2MHJSdU9uTEJOKzU3Y0VwMXowbkZzbjFncGg0VXhXNEYzSEw2R3gwOTByRUszak4yTUl5V25TRVRUUWpNemhkc24vZHVHYVJBREVNSlZGNDhZNWVyOW8vNEtIRmY1bGVqTEtJaDNVbmVCcnFwQlFFbEtIczcwZmpSc200clRYMjdmWWpHUFRqN2F1S0xocDcyd3ZXSnlMT3hSa3JpcE9RSVR4WkFCTlUvNFpSbmlOdVJidWhNZkZWcERGbUhoR3ppTjN1Z080Y2FTczFGQ1pKeDNFNS9nL0Y0cC85QXNRZHJpQ1l2VFgvWG5CSm1NZ1NMQXNydFpycFloZzQ4azI4UmZiQ1RPSUhMU1cwYUhSMG80WXZYK1JlUWNxa2xGQnFZNStsU2d3ZkJrdFViQSsxN2NqN3NVOVg0WVNOVmJvNWJCOUo0M08ydnVQSjF6Sk9FdFowUm43bzA2LzI5bUxEQ20zOGFxb0tkNHdTcnd6azcwNEVFd2Q3OFUwbm55UWJvbTNHejJJekU2K3lCdWZJL2cvbTJ3cmxDUzZlZWtBQUFBQVNVVk9SSzVDWUlJPSMkIzAxY2ZmMGJkNjBjMDQxOGViYzgzYWFhNWQ2YTZlNjk1\"}";

        /// <summary>
        /// The type initializer for 'Gdip' threw an exception.   at System.Drawing.SafeNativeMethods.Gdip.GdipCreateBitmapFromScan
        /// </summary>
        /// <returns></returns>
        public static KeyValuePair<string, string> Mock()
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(CodeList);
            return dict.ElementAt(new Random().Next(0, dict.Count - 1));
        }
        #endregion

    }
}