using System;
using WinHttp;
using System.Threading;
using System.IO;

namespace valid_invoice_checker
{
    class Program
    {
        static Semaphore smp;
        static Semaphore smp2;
        static void Main(string[] args)
        {
            smp = new Semaphore(1,1);
            smp2 = new Semaphore(Environment.ProcessorCount*2+1, Environment.ProcessorCount*2+1); //스레드 갯수
            Console.WriteLine("기준이 될 송장번호를 입력해주세요.");
            Console.WriteLine("예) 1234 5678 **** 와 같이 송장번호 앞 8자리 입니다.");
            Console.Write("8자리 기준번호 입력 > ");
            string mainCode = Console.ReadLine();
            Console.WriteLine("바탕화면에 결과 값이 저장됩니다.");
            int cnt = 1;
            for (int i=100; i<=9999899; i += 100)
            {
                smp2.WaitOne();
                run(i,i+100,mainCode);
                Console.WriteLine((cnt++) +"번째 Thread 생성 완료.");
            }


        }
        static public void run(int start, int end, string mainCode)
        {
            string real_mainCode = mainCode;
            WinHttpRequest wt = new WinHttpRequest();
            Thread t3 = new Thread(delegate ()
            {
                for (int i = start; i <= end; i++)
                {
                    mainCode = real_mainCode + i.ToString("0000");
                    wt.Open("GET", "https://www.hanjin.co.kr/kor/CMS/DeliveryMgr/WaybillResult.do?mCode=MN038&wblnum=" + mainCode + "&schLang=KR&wblnumText=");
                    wt.SetRequestHeader("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X x.y; rv:42.0) Gecko/20100101 Firefox/42.0");
                    wt.Send();
                    bool check = false;
                    if (wt.ResponseText.IndexOf("기본정보") != -1) check = true;
                    if (check)
                    {
                        Console.WriteLine(mainCode + " CHECK " + check);
                        smp.WaitOne();
                        File.AppendAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\" + DateTime.Now.ToString("HH-mm-dd") + " 결과.txt", mainCode + "\n");
                        smp.Release(1);
                    }
                }
                smp2.Release();
            });
            t3.Start();
        }
    }
}
