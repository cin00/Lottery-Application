using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

/*******************************************************************
 * Laboration 2 (Lottodragning) C# .NET DVGB07, vt24 av Henrik Hultgren
 * 
 * Beskrivning av program:
 * Användaren matar in en lottorad bestående av 7 siffror mellan 1 till och med 35. Dessa siffror sparas i ett HashSet.
 * Programmet slumpar fram lottorader begränsade av samma villkor som användarens och även dessa placeras i HashSets.
 * Alla HashSets (innehållandes slumprader) sparas sedan under i en List och snittet mellan genererade raderna och användarens lottorad tas fram för att jämföra antalet matchningar.
 * Notera att man endast kan mata in max 8 tecken i "antalet dragningar", detta för att förhindra problem med INT.MAX.
 * Man kan dock fortfarande mata in så pass höga värden att minnet rimligtvis tar slut (Testa hur högt ni kan köra! Min dator klarar upp till ca 8 miljoner dragningar.)
 * 
 * 
 * - Varför HashSets?
 *   Jag började lösa uppgiften enbart med List men insåg snabbt problematiken med att ej tillåta dubbletter samt att ordning ej spelar roll.
 *   Jag minns att jag lärde mig om HashSets<E> och eftersom HashSet är en mängd där ordning inte spelar roll,
 *   och dubbletter ej tillåts så kändes det som ett självklart val.
 *   Det fanns även inga krav om hur resurseffektivt programmet behövde vara.
 * 
 * 
 * Referenser:
 * https://stackoverflow.com/questions/150750/hashset-vs-list-performance
 * https://stackoverflow.com/questions/26931528/random-number-generator-with-no-duplicates
 * https://stackoverflow.com/questions/6391738/what-is-the-difference-between-hashsett-and-listt
 * https://stackoverflow.com/questions/13208517/should-i-await-async-calls-like-messagedialog-and-launcher-methods
 * https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1?view=net-8.0
 * https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.hashset-1?view=net-8.0
 * Kursboken
 * ChatGPT (ingen kod, endast enklare allmäna syntaxfrågor)
 * Jonathan Vestins videoföreläsningar
 *******************************************************************/

namespace Labb2_Lotto
{

    public sealed partial class MainPage : Page
    {
        private HashSet<int> myLotteryRow; //Hash set innehållandes användarens lottorad
        private int intInput; //Textinput konverterad till int
        private bool validInput; //Kontroll om input är ok
        private bool validNrOfDraws; //Kontroll om antalet dragningar är ok
        private const int minValue = 1; //Minsta tillåtna värde -> 1
        private const int maxValue = 35; //Högsta tillåtna värde -> 35
        private const int rowLength = 7; //Antalet siffror i en rad -> 7
        private int nrOfDraws; //Antalet dragningar  n > 0

        public MainPage()
        {
            this.InitializeComponent();

            //Initialisering
            myLotteryRow = new HashSet<int>();
            intInput = -1;
            nrOfDraws = 0;
            SevenCorrect.Text = "0";
            SixCorrect.Text = "0";
            FiveCorrect.Text = "0";
        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            validInput = true;
            SevenCorrect.Text = "0";
            SixCorrect.Text = "0";
            FiveCorrect.Text = "0";
            myLotteryRow.Clear();

            //Jämför validInput med NumberAdded returen och assignar resultatet i validInput
            validInput &= NumberAdded(Number1.Text, myLotteryRow);
            validInput &= NumberAdded(Number2.Text, myLotteryRow);
            validInput &= NumberAdded(Number3.Text, myLotteryRow);
            validInput &= NumberAdded(Number4.Text, myLotteryRow);
            validInput &= NumberAdded(Number5.Text, myLotteryRow);
            validInput &= NumberAdded(Number6.Text, myLotteryRow);
            validInput &= NumberAdded(Number7.Text, myLotteryRow);

            //Parsar text från 'antal dragningar-rutan'
            validNrOfDraws = int.TryParse(NrOfDraws.Text, out nrOfDraws);

            //Om lottoraden inte är korrekt angiven
            if (!validInput)
            {
                MessageDialog error_msg = new MessageDialog("Kontrollera att du skrivit in siffror mellan 1 och 35. (Ej dubletter!)\n");
                await error_msg.ShowAsync();
            }
            //Om angivet antal dragningar inte är korrekt
            else if (!validNrOfDraws || nrOfDraws <= 0)
            {
                MessageDialog error_msg2 = new MessageDialog("Kontrollera att du har minst 1 dragning!");
                await error_msg2.ShowAsync();
            }
            //Om allt är ok!
            else
            {
                try
                {
                    //allRandomRows innehåller returen från GenerateRows, dvs en Lista innehållandes alla HashSets
                    var allRandomRows = GenerateRows(nrOfDraws, rowLength, maxValue);
                    Compare(allRandomRows, myLotteryRow);
                }
                catch (Exception ex)
                {
                    //Generell except men mest tänkt på Out of memory exception - sker runt 8 miljoner dragningar på min dator
                    MessageDialog error = new MessageDialog($"Error: {ex.Message}\nTesta minska antalet dragningar!");
                    await error.ShowAsync();
                    myLotteryRow.Clear();
                }
            }
        }

        //Försöker parsa innehållet i min lottorad till ints och lägger till i myLotteryRow
        private bool NumberAdded(string textInput, HashSet<int> myLotteryRow)
        {
            if(int.TryParse(textInput, out intInput) && intInput >= minValue && intInput <= maxValue) 
            {
                return myLotteryRow.Add(intInput);
            }
            else
            {
                return false;
            }
        }

        //Metod som genererar nrOfDraws antal lottorader (HashSets) och lägger dessa i en List och returnerar denna
        private List<HashSet<int>> GenerateRows(int nrOfDraws, int rowLength, int maxValue)
        {
            Random rand = new Random();
            List<HashSet<int>> allRandomRows = new List<HashSet<int>>();

            for(int i = 0; i < nrOfDraws; i++) 
            {
                HashSet<int> randomRow = new HashSet<int>();
                while(randomRow.Count < rowLength)
                {
                    int randomNr = rand.Next(minValue, maxValue + 1);
                    randomRow.Add(randomNr);
                }
                allRandomRows.Add(randomRow);
            }

            return allRandomRows;
        }

        //Metod som jämför min lottorad med genererade lottorader
        private void Compare(List<HashSet<int>> allRandomRows, HashSet<int> myLotteryRow)
        {
            int sevenCorrect = 0;
            int sixCorrect = 0;
            int fiveCorrect = 0;

            foreach(var row in allRandomRows)
            {
                HashSet<int> temp = new HashSet<int>(row); //Skapar ett temporär HashSet för att inte ändra innehållet i "row". Behövs inte men vill inte ändra i orginalet.
                temp.IntersectWith(myLotteryRow); //Tar snittet mellan temp (row) och användarens lottorad

                //Increment ifall mängden temp nu innehåller 7, 6, eller 5 tecken.
                if(temp.Count == 7)
                {
                    sevenCorrect++;
                }
                else if(temp.Count == 6)
                {
                    sixCorrect++;
                }
                else if(temp.Count == 5)
                {
                    fiveCorrect++;
                }
            }

            //Ritar upp resultatet
            SevenCorrect.Text = sevenCorrect.ToString();
            SixCorrect.Text = sixCorrect.ToString();
            FiveCorrect.Text = fiveCorrect.ToString();

        }
    }
}
