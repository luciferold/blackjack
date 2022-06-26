using System;
using System.Collections.Generic;
using System.IO;
namespace BlackJack
{
    class Program
    {
        static Random rnd = new Random();
        static List<string> Suits, Deck, PlayerCards, PlayerCardSuits, RightHandCards, LeftHandCards, RightHandCardSuits, LeftHandCardSuits, DealerCards, DealerCardSuits;
        static int iBet_1, iBet_2, iCash, iPlayerTotal = 0, iDealerTotal = 0, iRightHandTotal, iLeftHandTotal, iAmountOfCardsPlayer, iAmountOfCardsDealer, iAmountOfCardsRightHand, iAmountOfCardsLeftHand, LeftOrRightHand, dd, iPlayerNextAction = 99;
        static bool ThereIsCardsWithTheSameValue, SplitIsActive, IsTakeACardRightHand, IsPlayerBlackJack, IsPlayerTurn, IsRightHandBusted, IsLeftHandBusted, IsDealerBlackJack;
        static void Main(string[] args)
        {
            Console.Title = "BlackJack by Denys";
            LoadOrNewGame();
        }
        static void GameStart()
        {
            ResetCash();
            Game();
        }
        static void SaveGame()
        {
            if (File.Exists("Save.txt"))
            {
                using StreamWriter Save = new StreamWriter("Save.txt");
                Save.WriteLine(iCash);
                Save.WriteLine("EUR");
            }
            else
            {
                Console.WriteLine("File not found");
            }
        }
        static void LoadGame()
        {
            if (File.Exists("Save.txt"))
            {
                using StreamReader LoadGame = new StreamReader("Save.txt");
                int iBank = int.Parse(LoadGame.ReadLine());
                if(iBank >= 1)
                {
                    iCash = iBank;
                    LoadGame.Close();
                    Game();
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("You have no save\nPress any key to start new game");
                    LoadGame.Close();
                    Console.ReadKey();
                    GameStart();
                }
            }
            else
            {
                Console.WriteLine("File not found");
            }
        }
        static void LoadOrNewGame()
        {
            int LoadOrNew = EnterInt("[1] Load game\n[0] New game\nYour action: ", 0, 1);
            switch (LoadOrNew)
            {
                case 1:
                    LoadGame();
                    break;
                case 0:
                    GameStart();
                    break;
            }
        }
        static void Game()
        {
            dd = 0;
            iAmountOfCardsDealer = 0;
            iAmountOfCardsPlayer = 0;
            IsPlayerTurn = true;
            SplitIsActive = false;
            Console.Clear();
            RefreshCards();
            RefreshCardSuits();
            PlayerCards = new List<string>();
            PlayerCardSuits = new List<string>();
            Bet();
            //GiveCard();
            //GiveCard();
            
            PlayerCards.Add("Queen");
            PlayerCardSuits.Add("♠");
            PlayerCards.Add("Ace");
            PlayerCardSuits.Add("♣");
            
            iAmountOfCardsPlayer = 2;
            ShowCurrentHand();
            CheckPlayerBlackJack();
            PlayerNextAction();
        }
        static string WhoWon(int iWhichHandTotal, int iBet)
        {
            string sResult = "";
            if (iWhichHandTotal > 21)
            {
                if (SplitIsActive == true)
                {
                    if (LeftOrRightHand == 0)
                    {
                        Console.WriteLine($"Left hand total: {CountTotal(iLeftHandTotal, LeftHandCards)} Busted! Lost: {iBet} EUR");
                    }
                    else
                    {
                        Console.WriteLine($"Right hand total: {CountTotal(iRightHandTotal, RightHandCards)} Busted! Lost: {iBet} EUR");
                    }
                }
                else
                {
                    Console.WriteLine($"Busted! Lost: {iBet} EUR");
                    DoYouWantToContinueOrRestart();
                }
            }
            else if (iWhichHandTotal > CountTotal(iDealerTotal, DealerCards))
            {
                iBet *= 2;
                iCash += iBet;
                if (SplitIsActive == true)
                {
                    if (LeftOrRightHand == 0)
                    {
                        Console.WriteLine($"Left hand total: {CountTotal(iLeftHandTotal, LeftHandCards)} > Dealer total: {CountTotal(iDealerTotal, DealerCards)} Win: {iBet} EUR");
                    }
                    else
                    {
                        Console.WriteLine($"Right hand total: {CountTotal(iRightHandTotal, RightHandCards)} > Dealer total: {CountTotal(iDealerTotal, DealerCards)} Win: {iBet} EUR");
                    }
                }
                else
                {
                    Console.WriteLine($"Your total: {CountTotal(iPlayerTotal, PlayerCards)} > Dealer total: {CountTotal(iDealerTotal, DealerCards)} Win: {iBet} EUR");
                }
            }
            else if (iWhichHandTotal < CountTotal(iDealerTotal, DealerCards))
            {
                if (SplitIsActive == true)
                {
                    if (LeftOrRightHand == 0)
                    {
                        Console.WriteLine($"Left hand total: {CountTotal(iLeftHandTotal, LeftHandCards)} < Dealer total: {CountTotal(iDealerTotal, DealerCards)} Lost: {iBet} EUR");
                    }
                    else
                    {
                        Console.WriteLine($"Right hand total: {CountTotal(iRightHandTotal, RightHandCards)} < Dealer total: {CountTotal(iDealerTotal, DealerCards)} Lost: {iBet} EUR");
                    }
                }
                else
                {
                    Console.WriteLine($"Your total: {CountTotal(iPlayerTotal, PlayerCards)} < Dealer total: {CountTotal(iDealerTotal, DealerCards)} Lost: {iBet_1} EUR");
                }
            }
            else
            {
                iCash += iBet;
                if (SplitIsActive == true)
                {
                    if (LeftOrRightHand == 0)
                    {
                        Console.WriteLine($"Left hand total: {CountTotal(iLeftHandTotal, LeftHandCards)} = Dealer total: {CountTotal(iDealerTotal, DealerCards)} Push!");
                    }
                    else
                    {
                        Console.WriteLine($"Right hand total: {CountTotal(iRightHandTotal, RightHandCards)} = Dealer total: {CountTotal(iDealerTotal, DealerCards)} Push!");
                    }
                }
                else
                {
                    Console.WriteLine($"Your total: {CountTotal(iPlayerTotal, PlayerCards)} = Dealer total: {CountTotal(iDealerTotal, DealerCards)} Push!");
                }
            }
            return sResult;
        }
        static void CheckWhoIsWinner()
        {
            LeftOrRightHand = 0; // 0 - left 1 - right
            if (SplitIsActive == true)
            {
                DealerTurn();
                WhoWon(CountTotal(iLeftHandTotal, LeftHandCards), iBet_1);
                LeftOrRightHand++;
                WhoWon(CountTotal(iRightHandTotal, RightHandCards), iBet_2);
                DoYouWantToContinueOrRestart();
            }
            else
            {
                DealerTurn();
            }
        }
        static void CheckWhoIsWinnerBlackJack()
        {
            if (IsDealerBlackJack == true && IsPlayerBlackJack == true)
            {
                iCash += iBet_1;
                Console.WriteLine("Both players have BlackJack! Push!");
                DoYouWantToContinueOrRestart();
            }
            else if (IsDealerBlackJack == true && IsPlayerBlackJack == false)
            {
                Console.WriteLine($"Dealer have BlackJack! You lost: {iBet_1} EUR");
                DoYouWantToContinueOrRestart();
            }
            else if (IsDealerBlackJack == false && IsPlayerBlackJack == true)
            {
                iBet_1 *= 2;
                iCash += iBet_1;
                Console.WriteLine($"You have BlackJack! You won: {iBet_1} EUR");
                DoYouWantToContinueOrRestart();
            }
        }
        static void DealerTurn()
        {
            DealerCardSuits = new List<string>();
            DealerCards = new List<string>();
            IsPlayerTurn = false;
            while (true)
            {
                if (CountTotal(iDealerTotal, DealerCards) <= 16)
                {
                    iAmountOfCardsDealer++;
                    GiveCard();
                }
                else
                {
                    break;
                }
            }
            ShowDealerHand();
            CheckDealerBusted();
            CheckDealerBlackJack();
            if (SplitIsActive == false)
            {
                CheckPlayerBlackJack();
                if (IsPlayerBlackJack == true || IsDealerBlackJack == true)
                {
                    CheckWhoIsWinnerBlackJack();
                }
                else
                {
                    WhoWon(CountTotal(iPlayerTotal, PlayerCards), iBet_1);
                }
            }
        }
        static void ShowDealerHand()
        {
            Console.Write("Dealer hand: ");
            for (int i = 0; i < DealerCards.Count; i++)
            {
                Console.Write($"{DealerCards[i]}{DealerCardSuits[i]} ");
            }
            ShowDealerTotal();
        }
        static void ShowDealerTotal()
        {
            Console.WriteLine($"\nDealer total: {CountTotal(iDealerTotal, DealerCards)}");
        }
        static void CheckDealerBusted()
        {
            if (CountTotal(iDealerTotal, DealerCards) > 21)
            {
                if (IsLeftHandBusted == false && IsRightHandBusted == false)
                {
                    iBet_1 *= 2;
                    iBet_2 *= 2;
                    iCash += iBet_1 + iBet_2;
                    Console.WriteLine($"Dealer busted! You won: {iBet_1 + iBet_2} EUR");
                    DoYouWantToContinueOrRestart();
                }
                else
                {
                    iBet_1 *= 2;
                    iCash += iBet_1;
                    Console.WriteLine($"Dealer busted! You won: {iBet_1}");
                    DoYouWantToContinueOrRestart();
                }
            }
        }
        static bool CheckPlayerBlackJack()
        {
            if (iAmountOfCardsPlayer == 2 && CountTotal(iPlayerTotal, PlayerCards) == 21)
            {
                return IsPlayerBlackJack = true;
            }
            else
            {
                return IsPlayerBlackJack = false;
            }
        }
        static bool CheckDealerBlackJack()
        {
            if (iAmountOfCardsDealer == 2 && CountTotal(iDealerTotal, DealerCards) == 21)
            {
                return IsDealerBlackJack = true;
            }
            else
            {
                return IsDealerBlackJack = false;
            }
        }
        static void ResetCash()
        {
            iCash = 500;
        }
        static void ShowCurrentCash()
        {
            Console.WriteLine($"Current cash: {iCash} EUR");
        }
        static void ShowCurrentBet()
        {
            if (SplitIsActive == true)
            {
                Console.WriteLine($"Total bet: {iBet_1 + iBet_2}");
            }
            else
            {
                Console.WriteLine($"Total bet: {iBet_1} EUR");
            }
        }
        static int EnterInt(string text, int Min, int Max)
        {
            Console.Write(text);
            int iNextAction;
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out iNextAction) == true)
                {
                    if (iNextAction >= Min && iNextAction <= Max)
                    {
                        return iNextAction;
                    }
                    else
                    {
                        Console.Write($"You have to choose between {Min} - {Max}: ");
                    }
                }
                else
                {
                    Console.Write($"You have to choose between {Min} - {Max}: ");
                }
            }
        }
        static int EnterBetAmount(string text, int Min, int Max)
        {
            Console.Write(text);
            int iAmount;
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out iAmount) == true)
                {
                    if (iAmount >= Min && iAmount <= Max)
                    {
                        return iAmount;
                    }
                    else
                    {
                        Console.Write($"Wrong amount of money {Min} EUR - {Max} EUR: ");
                    }
                }
                else
                {
                    Console.Write($"Wrong amount of money {Min} EUR - {Max} EUR: ");
                }
            }
        }
        static void Bet()
        {
            ShowCurrentCash();
            iBet_1 = EnterBetAmount("Enter an amount of bet: ", 1, iCash);
            iCash -= iBet_1;
            Console.Clear();
            ShowCurrentCash();
            ShowCurrentBet();
            Console.WriteLine("Press any key to start game");
            Console.ReadKey();
            Console.Clear();
        }
        static void IsCanSplit_1()
        {
            int iTheSamecards = 0;
            for (int i = 0; i < PlayerCards.Count; i++)
            {
                if (PlayerCards[i] == "Queen" || PlayerCards[i] == "Jack" || PlayerCards[i] == "King")
                {
                    iTheSamecards++;
                }
            }
            if (iTheSamecards >= 2)
            {
                ThereIsCardsWithTheSameValue = true;
            }
        }
        static void IsCanSplit_2()
        {
            switch (iAmountOfCardsPlayer)
            {
                case 2:
                    if (PlayerCards[0] == PlayerCards[1])
                    {
                        ThereIsCardsWithTheSameValue = true;
                    }
                    else
                    {
                        ThereIsCardsWithTheSameValue = false;
                    }
                    break;
                case 3:
                    if (PlayerCards[0] == PlayerCards[1] || PlayerCards[0] == PlayerCards[2] || PlayerCards[1] == PlayerCards[2])
                    {
                        ThereIsCardsWithTheSameValue = true;
                    }
                    else
                    {
                        ThereIsCardsWithTheSameValue = false;
                    }
                    break;
                case 4:
                    if (PlayerCards[0] == PlayerCards[1] || PlayerCards[0] == PlayerCards[2] || PlayerCards[0] == PlayerCards[3] || PlayerCards[1] == PlayerCards[2] || PlayerCards[1] == PlayerCards[3] || PlayerCards[2] == PlayerCards[3])
                    {
                        ThereIsCardsWithTheSameValue = true;
                    }
                    else
                    {
                        ThereIsCardsWithTheSameValue = false;
                    }
                    break;
                case 5:
                    if (PlayerCards[0] == PlayerCards[1] || PlayerCards[0] == PlayerCards[2] || PlayerCards[0] == PlayerCards[3] || PlayerCards[0] == PlayerCards[4] || PlayerCards[1] == PlayerCards[2] || PlayerCards[1] == PlayerCards[3] || PlayerCards[1] == PlayerCards[4] || PlayerCards[2] == PlayerCards[3] || PlayerCards[2] == PlayerCards[4] || PlayerCards[3] == PlayerCards[4])
                    {
                        ThereIsCardsWithTheSameValue = true;
                    }
                    else
                    {
                        ThereIsCardsWithTheSameValue = false;
                    }
                    break;
            }
        }
        static void Split()
        {
            if (ThereIsCardsWithTheSameValue == true && iCash - iBet_1 >= 0)
            {
                iBet_2 = iBet_1;
                iCash -= iBet_2;
                iRightHandTotal = 0; iLeftHandTotal = 0;
                SplitIsActive = true;
                IsRightHandBusted = false;
                IsLeftHandBusted = false;
                LeftHandCards = new List<string>();
                LeftHandCardSuits = new List<string>();
                RightHandCards = new List<string>();
                RightHandCardSuits = new List<string>();
                IsTakeACardRightHand = true;
                SplitHand();
                Console.WriteLine($"Total bet: {iBet_1 + iBet_2} EUR");
                ShowCurrentHand();
                PlayerNextAction();
            }
            else
            {
                YouCantSplit();
            }
        }
        static void SplitHand()
        {
            switch (iAmountOfCardsPlayer)
            {
                case 2:
                    SplitCardsPlayer(PlayerCards, LeftHandCards, 1, 0);
                    TakeACard(LeftHandCards, Deck, rnd.Next(Deck.Count));
                    SplitCardSuitsPlayer(PlayerCardSuits, LeftHandCardSuits, 1, 0);
                    TakeACardSuit(LeftHandCardSuits, Suits, rnd.Next(Suits.Count));
                    iAmountOfCardsLeftHand = 2;
                    SplitCardsPlayer(PlayerCards, RightHandCards, 1, 0);
                    TakeACard(RightHandCards, Deck, rnd.Next(Deck.Count));
                    SplitCardSuitsPlayer(PlayerCardSuits, RightHandCardSuits, 1, 0);
                    TakeACardSuit(RightHandCardSuits, Suits, rnd.Next(Suits.Count));
                    iAmountOfCardsRightHand = 2;
                    break;
                case 3:
                    SplitCardsPlayer(PlayerCards, LeftHandCards, 2, 0);
                    TakeACard(LeftHandCards, Deck, rnd.Next(Deck.Count));
                    SplitCardSuitsPlayer(PlayerCardSuits, LeftHandCardSuits, 2, 0);
                    TakeACardSuit(LeftHandCardSuits, Suits, rnd.Next(Suits.Count));
                    iAmountOfCardsLeftHand = 3;
                    SplitCardsPlayer(PlayerCards, RightHandCards, 1, 0);
                    TakeACard(RightHandCards, Deck, rnd.Next(Deck.Count));
                    SplitCardSuitsPlayer(PlayerCardSuits, RightHandCardSuits, 1, 0);
                    TakeACardSuit(RightHandCardSuits, Suits, rnd.Next(Suits.Count));
                    iAmountOfCardsRightHand = 2;
                    break;
                case 4:
                    SplitCardsPlayer(PlayerCards, LeftHandCards, 2, 0);
                    TakeACard(LeftHandCards, Deck, rnd.Next(Deck.Count));
                    SplitCardSuitsPlayer(PlayerCardSuits, LeftHandCardSuits, 2, 0);
                    TakeACardSuit(LeftHandCardSuits, Suits, rnd.Next(Suits.Count));
                    iAmountOfCardsLeftHand = 3;
                    SplitCardsPlayer(PlayerCards, RightHandCards, 2, 0);
                    TakeACard(RightHandCards, Deck, rnd.Next(Deck.Count));
                    SplitCardSuitsPlayer(PlayerCardSuits, RightHandCardSuits, 2, 0);
                    TakeACardSuit(RightHandCardSuits, Suits, rnd.Next(Suits.Count));
                    iAmountOfCardsRightHand = 3;
                    break;
                case 5:
                    SplitCardsPlayer(PlayerCards, LeftHandCards, 3, 0);
                    TakeACard(LeftHandCards, Deck, rnd.Next(Deck.Count));
                    SplitCardSuitsPlayer(PlayerCardSuits, LeftHandCardSuits, 3, 0);
                    TakeACardSuit(LeftHandCardSuits, Suits, rnd.Next(Suits.Count));
                    iAmountOfCardsLeftHand = 4;
                    SplitCardsPlayer(PlayerCards, RightHandCards, 2, 0);
                    TakeACard(RightHandCards, Deck, rnd.Next(Deck.Count));
                    SplitCardSuitsPlayer(PlayerCardSuits, RightHandCardSuits, 2, 0);
                    TakeACardSuit(RightHandCardSuits, Suits, rnd.Next(Suits.Count));
                    iAmountOfCardsRightHand = 3;
                    break;
            }
        }
        static List<string> SplitCardsPlayer(List<string> Cards, List<string> LeftOrRightHandCards, int iAmountOfCards, int index)
        {
            for (int i = 0; i < iAmountOfCards; i++)
            {
                LeftOrRightHandCards.Add(Cards[index]);
                Cards.RemoveAt(index);
            }
            return LeftOrRightHandCards;
        }
        static List<string> SplitCardSuitsPlayer(List<string> Suits, List<string> LeftOrRightHandCardSuits, int iAmountOfSuits, int index)
        {
            for (int i = 0; i < iAmountOfSuits; i++)
            {
                LeftOrRightHandCardSuits.Add(Suits[index]);
                Suits.RemoveAt(index);
            }
            return LeftOrRightHandCardSuits;
        }
        static void PlayerNextAction()
        {
            CheckPlayerBusted();
            IsMaxCards();
            IsCanSplit_1();
            if (ThereIsCardsWithTheSameValue == false)
            {
                IsCanSplit_2();
            }
            iPlayerNextAction = EnterInt("[1] Hit\n[2] Stand\n[3] Double-down\n[4] Split\n[5] Surrender \n[0] Exit \nWhat's your next step?: ", 0, 5);
            switch (iPlayerNextAction)
            {
                case 1:
                    Hit();
                    break;
                case 2:
                    Stand();
                    break;
                case 3:
                    DoubleDown();
                    break;
                case 4:
                    Console.Clear();
                    if (SplitIsActive == false)
                    {
                        Split();
                    }
                    else
                    {
                        YouCantSplit();
                    }
                    break;
                case 5:
                    Surrender();
                    break;
                default:
                    return;
            }
        }
        static void YouCantSplit()
        {
            Console.Clear();
            Console.WriteLine("You can't split");
            Console.WriteLine("Press any key to continue game");
            Console.ReadKey();
            Console.Clear();
            Console.WriteLine($"Total bet: {iBet_1 + iBet_2} EUR");
            ShowCurrentHand();
            PlayerNextAction();
        }
        static void Stand()
        {
            if (SplitIsActive == true)
            {
                if (IsTakeACardRightHand == true && iAmountOfCardsRightHand < 5)
                {
                    IsTakeACardRightHand = false;
                    Console.Clear();
                    ShowCurrentHand();
                    PlayerNextAction();
                }
                else
                {
                    CheckWhoIsWinner();
                    DoYouWantToContinueOrRestart();
                }
            }
            else
            {
                DealerTurn();
                DoYouWantToContinueOrRestart();
            }
        }
        static void DoubleDown()
        {
            if (SplitIsActive == true)
            {
                if (iCash - iBet_1 * 2 - iBet_2 * 2 >= 0 && dd == 0 && IsTakeACardRightHand == true)
                {
                    iBet_1 *= 2;
                    iBet_2 *= 2;
                    iCash -= iBet_1 + iBet_2;
                    Console.Clear();
                    Console.WriteLine($"Total bet: {iBet_1 + iBet_2} EUR");
                    dd++;
                    if (iAmountOfCardsPlayer == 4 || iAmountOfCardsPlayer == 8 || iAmountOfCardsPlayer == 12 || iAmountOfCardsPlayer == 16)
                    {
                        RefreshCardSuits();
                    }
                    TakeACard(RightHandCards, Deck, rnd.Next(Deck.Count));
                    iAmountOfCardsRightHand++;
                    TakeACardSuit(RightHandCardSuits, Suits, rnd.Next(Suits.Count));
                    TakeACard(LeftHandCards, Deck, rnd.Next(Deck.Count));
                    iAmountOfCardsLeftHand++;
                    TakeACardSuit(LeftHandCardSuits, Suits, rnd.Next(Suits.Count));
                    ShowCurrentHand();
                    CheckWhoIsWinner();
                    DoYouWantToContinueOrRestart();
                }
                else
                {
                    YouCantDoubleDown();
                }
            }
            else
            {
                if (iCash - iBet_1 >= 0 && dd == 0)
                {
                    iBet_1 *= 2;
                    iCash -= iBet_1;
                    Console.WriteLine($"Total bet: {iBet_1} EUR");
                    dd++;
                    GiveCard();
                    ShowCurrentHand();
                    CheckWhoIsWinner();
                    DoYouWantToContinueOrRestart();
                }
                else
                {
                    YouCantDoubleDown();
                }
            }
        }
        static void YouCantDoubleDown()
        {
            Console.Clear();
            Console.WriteLine("You can't double-down!");
            Console.WriteLine("Press any key to continue game");
            Console.ReadKey();
            Console.Clear();
            ShowCurrentHand();
            PlayerNextAction();
        }
        static void Surrender()
        {
            if(SplitIsActive == true)
            {
                if(iAmountOfCardsLeftHand <= 1 && iAmountOfCardsRightHand <= 1)
                {
                    iBet_1 /= 2;
                    iBet_2 /= 2;
                    iCash += iBet_1 + iBet_2;
                    Console.WriteLine($"You lost: {iBet_1 + iBet_2} EUR");
                    DoYouWantToContinueOrRestart();
                }
                else
                {
                    YouCantSurrender();
                }
            }
            if (iAmountOfCardsPlayer == 2)
            {
                iBet_1 /= 2;
                iCash += iBet_1;
                Console.WriteLine($"You lost: {iBet_1} EUR");
                DoYouWantToContinueOrRestart();
            }
            else
            {
                YouCantSurrender();
            }
        }
        static void YouCantSurrender()
        {
            Console.Clear();
            Console.WriteLine("You can't surrender!");
            Console.WriteLine("Press any key to continue game");
            Console.ReadKey();
            Console.Clear();
            ShowCurrentHand();
            PlayerNextAction();
        }
        static void DoYouWantToContinueOrRestart()
        {
            if (iCash <= 0)
            {
                iPlayerNextAction = EnterInt("[1] Restart\n[0] Exit\nWould you like to restart game?: ", 0, 1);
                switch (iPlayerNextAction)
                {
                    case 1:
                        GameStart();
                        break;
                    case 0:
                        Environment.Exit(0);
                        break;
                }
            }
            iPlayerNextAction = EnterInt("[1] Continue\n[0] Withdraw cash and exit\nYour action: ", 0, 1);
            switch (iPlayerNextAction)
            {
                case 1:
                    Game();
                    break;
                case 0:
                    SaveGame();
                    Environment.Exit(0);
                    break;
            }
        }
        static void CheckPlayerBusted()
        {
            if (SplitIsActive == true)
            {
                if (IsTakeACardRightHand == true)
                {
                    if (CountTotal(iRightHandTotal, RightHandCards) > 21)
                    {
                        IsTakeACardRightHand = false;
                        IsRightHandBusted = true;
                    }
                }
                else if (IsTakeACardRightHand == false)
                {
                    if (CountTotal(iLeftHandTotal, LeftHandCards) > 21)
                    {
                        IsLeftHandBusted = true;
                        if (CountTotal(iLeftHandTotal, LeftHandCards) > 21 && CountTotal(iRightHandTotal, RightHandCards) > 21)
                        {
                            Console.WriteLine($"Left hand busted! Lost: {iBet_1} EUR \nRight hand busted! Lost: {iBet_2} EUR");
                            DoYouWantToContinueOrRestart();
                        }
                        else 
                        {
                            CheckWhoIsWinner();
                        }
                    }
                }
            }
            else
            {
                if (CountTotal(iPlayerTotal, PlayerCards) > 21)
                {
                    Console.WriteLine($"Busted! Lost: {iBet_1} EUR");
                    DoYouWantToContinueOrRestart();
                }
                else if (CountTotal(iPlayerTotal, PlayerCards) == 21)
                {
                    Stand();
                }
            }
        }
        static void IsMaxCards()
        {
            if (SplitIsActive == true)
            {
                if (IsTakeACardRightHand == true)
                {
                    if (iAmountOfCardsRightHand == 5)
                    {
                        IsTakeACardRightHand = false;
                    }
                }
                else
                {
                    if (iAmountOfCardsLeftHand == 5)
                    {
                        CheckWhoIsWinner();
                    }
                }
            }
            else
            {
                if (iAmountOfCardsPlayer == 5 && CountTotal(iPlayerTotal, PlayerCards) != 21)
                {
                    Stand();
                }
            }
        }
        static void Hit()
        {
            if (SplitIsActive == true)
            {
                if (IsTakeACardRightHand == true)
                {
                    iAmountOfCardsRightHand++;
                }
                else
                {
                    iAmountOfCardsLeftHand++;
                }
            }
            else
            {
                iAmountOfCardsPlayer++;
                if (iAmountOfCardsPlayer == 4 || iAmountOfCardsPlayer == 8 || iAmountOfCardsPlayer == 12)
                {
                    RefreshCardSuits();
                }
            }
            GiveCard();
            Console.Clear();
            ShowCurrentHand();
            PlayerNextAction();
        }
        static void GiveCard()
        {
            if (IsPlayerTurn == true)
            {
                if (SplitIsActive == true)
                {
                    if (IsTakeACardRightHand == true)
                    {
                        TakeACard(RightHandCards, Deck, rnd.Next(Deck.Count));
                        TakeACardSuit(RightHandCardSuits, Suits, rnd.Next(Suits.Count));
                    }
                    else
                    {
                        TakeACard(LeftHandCards, Deck, rnd.Next(Deck.Count));
                        TakeACardSuit(LeftHandCardSuits, Suits, rnd.Next(Suits.Count));
                    }
                }
                else
                {
                    TakeACard(PlayerCards, Deck, rnd.Next(Deck.Count));
                    TakeACardSuit(PlayerCardSuits, Suits, rnd.Next(Suits.Count));
                }
            }
            else
            {
                TakeACard(DealerCards, Deck, rnd.Next(Deck.Count));
                TakeACardSuit(DealerCardSuits, Suits, rnd.Next(Suits.Count));
            }
        }
        static List<string> TakeACard(List<string> WhichHandCards, List<string> AllCards, int index_1)
        {
            WhichHandCards.Add(AllCards[index_1]);
            AllCards.RemoveAt(index_1);
            return WhichHandCards;
        }
        static List<string> TakeACardSuit(List<string> WhichHandCardSuits, List<string> AllSuits, int index_2)
        {
            WhichHandCardSuits.Add(AllSuits[index_2]);
            AllSuits.RemoveAt(index_2);
            return WhichHandCardSuits;
        }
        static void ShowCurrentHand()
        {
            if (SplitIsActive == true)
            {
                Console.Write($"Left hand: ");
                for (int i = 0; i < LeftHandCards.Count; i++)
                {
                    Console.Write($"{LeftHandCards[i]}{LeftHandCardSuits[i]} ");
                }
                Console.Write($"\nRight hand: ");
                for (int i = 0; i < RightHandCards.Count; i++)
                {
                    Console.Write($"{RightHandCards[i]}{RightHandCardSuits[i]} ");
                }
                ShowCurrentTotal();
            }
            else
            {
                Console.Write($"Current hand: ");
                for (int i = 0; i < PlayerCards.Count; i++)
                {
                    Console.Write($"{PlayerCards[i]}{PlayerCardSuits[i]} ");
                }
                ShowCurrentTotal();
            }
        }
        static void ShowCurrentTotal()
        {
            if (SplitIsActive == true)
            {
                Console.Write($"\nLeft hand total: {CountTotal(iLeftHandTotal, LeftHandCards)} \nRight hand total: {CountTotal(iRightHandTotal, RightHandCards)}\n");
                CheckPlayerBusted();
            }
            else
            {
                Console.WriteLine($"\nCurrent total: {CountTotal(iPlayerTotal, PlayerCards)}");
                CheckPlayerBusted();
            }
        }
        static int CountTotal(int WhichHandTotal, List<string> WhichHandCards)
        {
            for (int i = 0; i < WhichHandCards.Count; i++)
            {
                switch (WhichHandCards[i])
                {
                    case "2":
                        WhichHandTotal += 2;
                        break;
                    case "3":
                        WhichHandTotal += 3;
                        break;
                    case "4":
                        WhichHandTotal += 4;
                        break;
                    case "5":
                        WhichHandTotal += 5;
                        break;
                    case "6":
                        WhichHandTotal += 6;
                        break;
                    case "7":
                        WhichHandTotal += 7;
                        break;
                    case "8":
                        WhichHandTotal += 8;
                        break;
                    case "9":
                        WhichHandTotal += 9;
                        break;
                    case "Ace":
                        if (WhichHandTotal <= 10)
                        {
                            WhichHandTotal += 11;
                        }
                        else
                        {
                            WhichHandTotal += 1;
                        }
                        break;
                    default:
                        WhichHandTotal += 10;
                        break;
                }
            }
            return WhichHandTotal;
        }

        static void RefreshCardSuits()
        {
            Suits = new List<string>();
            string[] CardSuits =
            {
                "♦","♥","♠","♣",
            };
            for (int i = 0; i < CardSuits.Length; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    Suits.Add(CardSuits[i]);
                }
            }
        }
        static void RefreshCards()
        {
            Deck = new List<string>();
            string[] CardsName =
            {
                "2", "3", "4", "5", "6", "7", "8","9","10","Ace","King","Queen","Jack",
            };
            for (int j = 0; j < CardsName.Length; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    Deck.Add(CardsName[j]);
                }
            }
        }
    }
}
