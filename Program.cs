using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Net;
using System.IO;
using System.Numerics;

namespace serverApp
{
    class Program
    {
        public class JsonRSP
        {
            public double received { get; set; }
            public double balance { get; set; }

        }
        static string toFixed(double sum)
        {
            sum /= 100000000;
            return sum.ToString("N8");
        }
        public static void HttpListener(string[] prefixes)
        {
            HttpListener listener = new HttpListener();
            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }
            listener.Start();

            HttpListenerContext context;
            HttpListenerRequest request_h;
            HttpListenerResponse response_h;
            Stream output;
            byte[] buffer;

            BigInteger num, previous, next,startPrivKey;
            BigInteger max = BigInteger.Parse("904625697166532776746648320380374280100293470930272690489102837043110636675");
            byte[] hash = new byte[32];
            String hashKey,privKey,privKey_C,bitAddr,bitAddr_C,rsp,searchKey = String.Empty;
            Key privateKey;
            PubKey publicKey;
            Key privateKey_C;
            PubKey publicKey_C;
            WebRequest request;
            WebResponse response;
            Stream dataStream;
            StreamReader reader;
            JsonRSP obj;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            while (true)
            {
                // 1 - 115792089237316195423570985008687907852837564279074904382605163141518161494337 bitcoin privatekey space
                context = listener.GetContext();
                request_h = context.Request;
                response_h = context.Response;
                if (request_h.RawUrl.ToString() == "/favicon.ico") continue;
                if (request_h.RawUrl.ToString().StartsWith("/5H") || request_h.RawUrl.ToString().StartsWith("/5K") || request_h.RawUrl.ToString().StartsWith("/5J") || request_h.RawUrl.ToString().StartsWith("/K") || request_h.RawUrl.ToString().StartsWith("/L"))
                {
                    var secret= new BitcoinSecret(request_h.RawUrl.Remove(0, 1));
                    var key = secret.PrivateKey;
                    searchKey = key.PubKey.GetAddress(Network.Main).ToString();
                    byte[] b = key.ToBytes();
                    String hashK = String.Empty;
                    foreach (byte h in b) hashK += h.ToString("x2");
                    hashK = "0" + hashK;
                    num = BigInteger.Parse(hashK, System.Globalization.NumberStyles.HexNumber);
                    num = num / 128;
                    num = num + 1;
                }
                else
                num = BigInteger.Parse(request_h.RawUrl.Remove(0,1));
                previous = num - 1;
                if (previous == 0) previous = 1;
                next = num + 1;
                if (next > max) next = max;
                startPrivKey = (num - 1) * 128+1;
                StringBuilder sb = new StringBuilder();
                //***************************************************
                sb.Append("<!DOCTYPE html>");
                sb.Append("<html>");
                sb.Append("<head>");
                sb.Append("<title>BTC_WebServer</title>");
                sb.Append("<style>body{font-size:9.3pt;font-family:'Open Sans',sans-serif;}a{text-decoration:none}a:hover {text-decoration: underline}lol: target {background: #ccffcc; }</style>");
                sb.Append("</head>");
                sb.Append("<body>");
                sb.Append("<h1>Bitcoin private key database</h1>");
                sb.Append("<h3>Page " + num + " out of 904625697166532776746648320380374280100293470930272690489102837043110636675</h3>");
                sb.Append("<a href='/"+previous+"'>previous</a> | ");
                sb.Append("<a href='/"+next+"'>next</a>");
                sb.Append("<br>");
                sb.Append("<pre class='keys'><strong>Private Key</strong>                                            <strong>Address</strong>                                <strong>Compressed Address</strong>                       <strong>Private Key Compressed</strong><br>");
                for (int i = 0; i < 128; i++)
                {
                    if (startPrivKey.ToString("x2").Length == 65)
                        hashKey = startPrivKey.ToString("x2").Remove(0, 1);
                    else
                        hashKey = startPrivKey.ToString("x2");
                    if (hashKey.Length < 64) hashKey = hashKey.PadLeft(64, '0');
                    int step = 0;
                    for (int a = 0; a < 32; a++)
                    {
                        hash[a] = Convert.ToByte(hashKey.Substring(step, 2), 16);
                        step += 2;
                    }
                    
                    privateKey = new Key(hash,-1,false);
                    privateKey_C = new Key(hash);
                    publicKey = privateKey.PubKey;
                    publicKey_C = privateKey_C.PubKey;                    
                    privKey = privateKey.GetWif(Network.Main).ToString();
                    privKey_C = privateKey_C.GetWif(Network.Main).ToString();
                    bitAddr = publicKey.GetAddress(Network.Main).ToString();
                    bitAddr_C = publicKey_C.GetAddress(Network.Main).ToString();

                    request = WebRequest.Create("https://blockchain.info/q/addressbalance/" + bitAddr);
                    response = request.GetResponse();
                    dataStream = response.GetResponseStream();
                    reader = new StreamReader(dataStream);
                    rsp = reader.ReadToEnd();
                    Console.WriteLine(rsp);
                    //dynamic data = JsonConvert.DeserializeObject(rsp);

                    if (bitAddr == searchKey || bitAddr_C == searchKey)
                    sb.Append("<lol>" + privKey + "</lol>&nbsp;&nbsp;&nbsp;&nbsp;<lol style='display:inline-block;width:230px;color:#145A32;'><a href='https://bitaps.com/"+ bitAddr + "'>" +"<strong>"+ bitAddr+ "</strong>"+ "</a></lol>&nbsp;&nbsp;&nbsp;&nbsp;<lol style='display:inline-block;width:230px;color:#D35400;'><a href='https://bitaps.com/" + bitAddr_C+"'>"+"<strong>"+ bitAddr_C +"</strong>"+ "</a></lol>&nbsp;&nbsp;&nbsp;&nbsp;<lol style='color:#145A32;'>" +"&nbsp;&nbsp;&nbsp;&nbsp;<lol>"+privKey_C+"</lol></br>");
                    else
                    sb.Append("<lol>" + privKey + "</lol>&nbsp;&nbsp;&nbsp;&nbsp;<lol style='display:inline-block;width:230px;color:#145A32;'><a href='https://bitaps.com/" + bitAddr + "'>" + bitAddr + "</a>" + "&nbsp<span>" + rsp + "</span>&nbsp" + "</lol>&nbsp;&nbsp;&nbsp;&nbsp;<lol style='display:inline-block;width:230px;color:#D35400;'><a href='https://bitaps.com/" + bitAddr_C + "'>" + bitAddr_C + "</a></lol>&nbsp;&nbsp;&nbsp;&nbsp;<lol style='color:#145A32;'>" + "&nbsp;&nbsp;&nbsp;&nbsp;<lol>" + privKey_C + "</lol></br>");
                    sb.Append("<lol>" + privKey + "</lol>&nbsp;&nbsp;&nbsp;&nbsp;<lol style='display:inline-block;width:230px;color:#145A32;'><a href='https://bitaps.com/" + bitAddr + "'>" + bitAddr + "</a>" + "&nbsp<span>" + rsp + "</span>&nbsp" + "</lol>&nbsp;&nbsp;&nbsp;&nbsp;<lol style='display:inline-block;width:230px;color:#D35400;'><a href='https://bitaps.com/" + bitAddr_C + "'>" + bitAddr_C + "</a></lol>&nbsp;&nbsp;&nbsp;&nbsp;<lol style='color:#145A32;'>" + "&nbsp;&nbsp;&nbsp;&nbsp;<lol>" + privKey_C + "</lol></br>");
                    sb.Append("<lol>" + privKey + "</lol>&nbsp;&nbsp;&nbsp;&nbsp;<lol style='display:inline-block;width:230px;color:#145A32;'><a href='https://bitaps.com/" + bitAddr + "'>" + bitAddr + "</a>" + "&nbsp<span>" + rsp + "</span>&nbsp" + "</lol>&nbsp;&nbsp;&nbsp;&nbsp;<lol style='display:inline-block;width:230px;color:#D35400;'><a href='https://bitaps.com/" + bitAddr_C + "'>" + bitAddr_C + "</a></lol>&nbsp;&nbsp;&nbsp;&nbsp;<lol style='color:#145A32;'>" + "&nbsp;&nbsp;&nbsp;&nbsp;<lol>" + privKey_C + "</lol></br>");
                    sb.Append(rsp);
                    if (startPrivKey == BigInteger.Parse("115792089237316195423570985008687907852837564279074904382605163141518161494336")) break;
                    startPrivKey++;
                }
                sb.Append("<br><a href='/" + previous + "'>previous</a> | ");
                sb.Append("<a href='/" + next + "'>next</a>");
                sb.Append("</pre><br>");
                sb.Append("</body>");
                sb.Append("</html>");
                //****************************************************
                buffer = Encoding.UTF8.GetBytes(sb.ToString());
                response_h.ContentLength64 = buffer.Length;
                output = response_h.OutputStream;
                output.Write(buffer, 0, buffer.Length);
            }
        }
        public static void Main(string[] args)
        {
           /*var ecret = new BitcoinSecret("5Km2kuu7vtFDPpxywn4u3NLpbr5jKpTB3jsuDU2KYEqetqj84qw");
            var key = secret.PrivateKey;
            byte[] b = key.ToBytes();
            String hash = String.Empty;
            foreach (byte h in b) hash += h.ToString("x2");
            Console.WriteLine(hash);
            Console.ReadKey(true);*/
            Console.WriteLine("WebServer Listening..");
            HttpListener(new[] { "http://localhost:3333/" });
        }
    }
}
