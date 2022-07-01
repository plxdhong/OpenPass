using System;
using System.Web;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Pass.Services
{
    public class HtmlIo
    {
        private readonly ILogger<HtmlIo> _logger;
        public HtmlIo(ILogger<HtmlIo> logger)
        {
            _logger = logger;
        }
        public async Task<string> GetHtmlTempAsync(string message)
        {
            try
            {
                using(Stream myStream = new FileStream("./Pages/EmailTemp/1.html", FileMode.Open)){
                    Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                    StreamReader myStreamReader = new StreamReader(myStream, encode);
                    string strhtml = await myStreamReader.ReadToEndAsync();
                    string stroutput = strhtml.Replace("$[message]$", message);
                    myStream.Close();
                    return stroutput;
                }
            }
            catch
            {
                _logger.LogError("Pass.Services.HtmlIo.GetHtmlTemp 读取文件错误");
                return "Error";
            }
        }
    }
}
