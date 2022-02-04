using System;
using System.Threading.Tasks;

namespace DemoWorkerService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // elso param:
            // a megfigyelni valo mappa abszolut utvonala

            // masodik param:
            // a futtatando .zip file-ok nevenek mintaja
            // pl.:  mokus*  azt jelenti, hogy azokat a .zip file-okat figyelje, amik
            // ugy kezdodnek, hogy "mokus", utana lehet barmi, es ugy vegzodnek, hogy .zip
            // pl.: mukosoknakHosszuAFarka.zip

            // harmadik param:
            // mennyi milisecet varjon a .zip file-ok masolasanak elkeszuleseig
            // lefele a kovetkezo 2 hatvanyra kerekitodik
            // pl. a 8 az 2 + 4 + 8 = 14 milisec-et var
            // pl. a 7 az 2 + 4 = 6 milisec-et var
            await new App(@"C:\Users\Gerinnyo12\Desktop\Watched_Folder", "Asd*", 1000).Start();
        }
    }
}
