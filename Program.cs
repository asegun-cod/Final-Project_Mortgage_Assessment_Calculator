//Author:                   Oluwasegun Apejoye (N0966184)
//Program Name:             Mortgage Deal Estimator
//Function of This Program: This program allows users (customers) to enter their details including their 
//                          financial details and the program return two mortgage deals to them. User's 
//                          data are then stored in a file (.csv) for future processing 

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

[Serializable]
class MortgageDeal
{
    private double dealInterest;
    private double standardInterest;
    public double toBerepaidAmount;
    private int feesPaidUpfront;
    private int dealYear;
    public int mortgageRepaymentYear;

    //The constructor
    public MortgageDeal(int dealYear, double dealInterest, double fixedInterest, double toBerepaidAmount, int mortgageRepaymentYear, int feesPaidUpfront)
    {
        this.dealYear = dealYear;
        this.dealInterest = dealInterest;
        this.standardInterest = fixedInterest;
        this.mortgageRepaymentYear = mortgageRepaymentYear;
        this.toBerepaidAmount = toBerepaidAmount;
        this.feesPaidUpfront = feesPaidUpfront;
    }

    //calculate the fixed interest rate for the created deal    
    public double getFixedInterest()
    {
        double value = Program.compoundInterestCalc(toBerepaidAmount, dealInterest, dealYear); //Calculate the fixed interest for the first years of the deal
        return value;
    }

    //calculate the compount amount 
    //(property value + interest) for the fixed period for the deal
    private double getFixedCompoundAmount()
    {
        double value = Program.calculateCompoundAmount(toBerepaidAmount, dealInterest, dealYear); //Calculate the fixed compound amount for the first years of the deal
        return value;
    }

    public double getTotalCompoundAmount()
    {
        double value = Program.calculateCompoundAmount(getFixedCompoundAmount(), standardInterest, (mortgageRepaymentYear - dealYear));//Calculate the interest for the remaining years of the mortgage        
        return value;
    }

    //Compute the Total (property minus the initial 10%/15% deposit vaule plus interest ) that customers'll repay 
    //over full term. Note this is not the actual total amount to pay over 
    //the full course of the mortage. as it doesnt account for the addition fee such S STAMPDUTY etc.
    private double getTotalcompoundinterest()
    {
        double value = getTotalCompoundAmount() - toBerepaidAmount;
        return value;
    }

    public double totalToPay()
    {
        double value = getTotalCompoundAmount() + feesPaidUpfront;
        return value;
    }

    public double monthlyRepayment()
    {
        double monthsInAYear = 12;
        double value = totalToPay() / monthsInAYear / mortgageRepaymentYear;
        return value;
    }

    //funtion to print the fixed interest for the created deal
    public void printDealFixedInterest()
    {
        Console.WriteLine("Your first {0}year interest rate is {1}", dealYear, Math.Round(getFixedInterest(), 2));
    }

    //funtion to print the Total interest incurred on this mortgage
    public void printDealtotalInterest()
    {
        Console.WriteLine("Your Total incurred interest for our {0}year deal is {1}", dealYear, Math.Round(getTotalcompoundinterest(), 2));
    }

    //function to print deals' Total amount to repay
    public void printDealTotalToPay()
    {
        Console.WriteLine("The total amount you will pay for our {0}year deal is £{1}", dealYear, Math.Round(totalToPay(), 2));
    }

    //function to print deals' monthly repayment
    public void printDealMonthlyRepayment()
    {
        Console.WriteLine("Your Monthly repayment for our {0}year deal is £{1}", dealYear, Math.Round(monthlyRepayment(), 2));
    }

    //Print out Summery of the needed detals from this deal
    public void printDealOutput()
    {
        Console.WriteLine("{0} YEARS FIXED INTEREST RATE DEAL", dealYear);
        Console.WriteLine("The first {0} years interest rate is                             £{1}", dealYear, dealInterest);
        Console.WriteLine("Interest rate for the remaining years of this mortgage is      £{1}", dealYear, standardInterest);
        Console.WriteLine("Your Total incurred interest for this deal is                  £{0}", Math.Round(getTotalcompoundinterest(), 2));
        Console.WriteLine("The total amount you will pay for this deal is                 £{0}", Math.Round(totalToPay(), 2));
        Console.WriteLine("Your Monthly repayment for this deal is                        £{0}", Math.Round(monthlyRepayment(), 2));
    }
}

class Program
{
    //------------------------------------------------General Function -------------------------------------------//
    //function that displays a prompt message, and then reads an integer 
    //between specified lower and upper bounds. If the input is not an 
    //integer within those bounds, then the user is re-prompted
    static int ReadUpperLowerInteger(string prompt, int Lower, int Upper)
    {
        bool check;
        int result;

        do
        {
            check = int.TryParse(readString(prompt), out result);
        }
        while (!check || (result < Lower || result > Upper));
        return result;
    }

    static string ReadValidEmailAddress(string Prompt)
    {
        string emailAddress;
        Console.Write(Prompt);
        do
        {
            emailAddress = Console.ReadLine();

            if (new EmailAddressAttribute().IsValid(emailAddress) &&
                (emailAddress != null) && !emailAddress.Contains(",")) return emailAddress;
            Console.Write("Invalid email address! Please enter a valid email address: ");
        }
        while (true);
    }

    static string ReadStringAorB(string Prompt, string a, string b)
    {
        string Entered;
        do
        {
            Entered = readString(Prompt).ToLower();
        }
        while (!(Entered == a || Entered == b));
        return Entered;
    }

    //Function that displays a prompt message and re-prompted user
    //when empty string is entered 
    static string readString(string Prompt)
    {
        string Entered;
        do
        {
            Console.Write(Prompt);
            Entered = Console.ReadLine();
        }
        while (Entered == "");
        return Entered;
    }

    //Check if the filepath does exist and throws an exception if it doesn't
    static void checkIfFileExist(string Filepath)
    {
        if (!File.Exists(Filepath))
            throw new Exception($"The file in specified path ({Filepath}) does not exist or has been renamed/moved");
    }

    static double ReadMinAndMaxDouble(string prompt, double Lower, double Upper)
    {
        double result;
        bool check;
        do
        {
            check = double.TryParse(readString(prompt), out result);
        }
        while (!check || (result < Lower || result > Upper));
        return result;
    }

    //---------------------------------------Configuraration file Functions-------------------------------------------------------//
    //Take the configuration file and its delimiter as parameter and return a dictionary containing all elements in the file
    static Dictionary<string, double> returnConfigFileAsDictionary(string ConfigFilePath, string delimiterAsString)
    {
        string[] configFile = File.ReadAllLines(ConfigFilePath); //open the file and read all its line into an array.
        List<string> detailsCollection = new List<string>();
        Dictionary<string, double> detailsInDict = new Dictionary<string, double>();
        int lineForFileHeader = 4;

        //open the array and read only the lines with entries into a list
        for (int i = lineForFileHeader; i < configFile.Length; i++)
        {
            if (!String.IsNullOrWhiteSpace(configFile[i]))
                detailsCollection.Add(configFile[i]);
        }

        //Loop through each line in the list, split them using ":" delimiter and trim out white lines; 
        //and assign the details name into a dictionary using the name as "key" and value as "value"
        foreach (var line in detailsCollection)
        {
            string[] individualDetail = line.Split(delimiterAsString);    // split each of the line using ":" delimiter and trim out white lines
            detailsInDict[individualDetail[0].Trim().ToUpper()] = double.Parse(individualDetail[1]);
        }
        return detailsInDict;
    }

    //-------------------------------------------- Mortgage functions -----------------------------------------------------//
    //function to compute compount Amount (interest + principal) compounded x times (set to 1 by default) annually
    //function to compute compount interest compounded x times (set to 1 by default) annually
    public static double compoundInterestCalc(double amount, double interestRate, int compoundYears, int xTimes = 1)
    {
        return calculateCompoundAmount(amount, interestRate, compoundYears) - amount;
    }

    public static double calculateCompoundAmount(double amount, double interestRate, int compoundYears, int xTimes = 1)
    {
        return amount * Math.Pow((1 + interestRate / 100 / xTimes), (compoundYears * xTimes));
    }

    public static int feesPaidUpfrontCal(int stampDuty, int legalFee)
    {
        int feesPaidUpfront = stampDuty + legalFee;
        return feesPaidUpfront;
    }

    public static int StampDutyCal(int propertyValue, int StampDutyThreshold,
        int StampDutyLessThanThreshold, int StampDutyMoreThanThreshold)
    {
        if (propertyValue < StampDutyThreshold) return StampDutyLessThanThreshold;
        return StampDutyMoreThanThreshold;
    }

    public static int LegalFeeCal(int propertyValue, int legalthreshold, int lessFeeLessThanthreshold, int legalFeeMoreThanTheThreshold)
    {
        if (propertyValue < legalthreshold) return lessFeeLessThanthreshold;
        return legalFeeMoreThanTheThreshold;
    }
    //function to print the additional Mortgage fees
    static void printAdditionalCost(int stampDuty, int legalFee, int totalUpFrontCost)
    {
        Console.WriteLine("The additional cost added to the total amount of each Mortgage deal is");
        Console.WriteLine("Stamp Duty                                                       £{0}", stampDuty);
        Console.WriteLine("Legal Fee                                                        £{0}", legalFee);
        Console.WriteLine("______________________________________________________________________");
        Console.WriteLine("Total additional cost work out to be                             £{0}", totalUpFrontCost);
    }

    static void Main()
    {
        //variable declaration
        string customerName, customerEmailAddress, userData, dealOpt1, dealOpt2, choosenDeal, newLine = "\n",
            ConfigFileName = "ConfigFile.txt", userDataFile = "userDataFile.csv", configFiledelimiter = ":";
        int customerAge, propertyValue, mortgageRepaymentYear, stampDuty, legalFee, feesPaidUpfront;
        double deposit, toBerepaidAmount;
        Dictionary<string, double> detailsInDict = new Dictionary<string, double>();

        //Load the Constant parameters from the Configuration file. This all catch any exception encountered  
        try
        {
            checkIfFileExist(ConfigFileName);
            detailsInDict = returnConfigFileAsDictionary(ConfigFileName, configFiledelimiter);
            //Variable declaration from the configuration settings
            int MINIMUM_AGE = Convert.ToInt32(detailsInDict["MINIMUM_AGE"]), MAXIMUM_AGE = Convert.ToInt32(detailsInDict["MAXIMUM_AGE"]),
                BORROW_THRESHOLD = Convert.ToInt32(detailsInDict["BORROW_THRESHOLD"]), FIRST_DEAL_YEAR = Convert.ToInt32(detailsInDict["DEAL_ONE_YEAR"]),
                SECOND_DEAL_YEAR = Convert.ToInt32(detailsInDict["DEAL_TWO_YEAR"]), LEGAL_THRESHOLD = Convert.ToInt32(detailsInDict["LEGAL_THRESHOLD"]),
                LEGAL_FEE_LESS_THAN_THRESHOLD = Convert.ToInt32(detailsInDict["LEGAL_FEE_LESS_THAN_THRESHOLD"]),
                LEGAL_FEE_MORE_THAN_THRESHOLD = Convert.ToInt32(detailsInDict["LEGAL_FEE_MORE_THAN_THRESHOLD"]),
                STAMP_DUTY_THRESHOLD = Convert.ToInt32(detailsInDict["STAMP_DUTY_THRESHOLD"]),
                STAMPDUTY_LESS_THAN_THRESHOLD = Convert.ToInt32(detailsInDict["STAMPDUTY_LESS_THAN_THRESHOLD"]),
                STAMP_DUTY_MORE_THAN_THRESHOLD = Convert.ToInt32(detailsInDict["STAMP_DUTY_MORE_THAN_THRESHOLD"]),
                MINIMUM_MORTGAGE_YEAR = Convert.ToInt32(detailsInDict["MINIMUM_MORTGAGE_YEAR"]),
                MAXIMUM_MORTGAGE_YEAR = Convert.ToInt32(detailsInDict["MAXIMUM_MORTGAGE_YEAR"]),
                MINIMUM_BORROW = Convert.ToInt32(detailsInDict["MINIMUM_BORROW"]), MAXIMUM_BORROW = Convert.ToInt32(detailsInDict["MAXIMUM_BORROW"]);
            double FIRST_DEAL_INITIAL_INTEREST = detailsInDict["DEAL_ONE_INITIAL_INTEREST"], SECOND_DEAL_INITIAL_INTEREST = detailsInDict["DEAL_TWO_INITIAL_INTEREST"],
                STANDARD_INTEREST = detailsInDict["STANDARD_INTEREST"],
                MINIMUM_PERCENT_OF_PROPERTY_VALUE_BELOW_THRESHOLD = detailsInDict["MINIMUM_PERCENT_OF_PROPERTY_VALUE_BELOW_THRESHOLD"],
                MINIMUM_PERCENT_OF_PROPERTY_VALUE_ABOVE_THRESHOLD = detailsInDict["MINIMUM_PERCENT_OF_PROPERTY_VALUE_ABOVE_THRESHOLD"],
                MAXIMUM_PERCENT_OF_PROPERTY_VALUE_THRESHOLD = detailsInDict["MAXIMUM_PERCENT_OF_PROPERTY_VALUE_THRESHOLD"];

            Console.WriteLine("==================================================================");
            Console.WriteLine("WELCOME TO OLUAPEJ MORTGAGE BANK'S MORTGAGE DEAL CALCULATOR");
            Console.WriteLine("Just provide few deals and we will show you the great deals we have for you.");
            Console.WriteLine("==================================================================");
            Console.WriteLine("Let's start by getting you to the right place:");

            //Get customer details
            customerName = readString("Enter Your name: ");
            customerAge = ReadUpperLowerInteger($"Enter Your Age (Between {MINIMUM_AGE} and {MAXIMUM_AGE} years): ", MINIMUM_AGE, MAXIMUM_AGE);
            propertyValue = ReadUpperLowerInteger($"Enter the likely property price Minimum of £{MINIMUM_BORROW} " +
                $"and Maximum of £{MAXIMUM_BORROW}: £", MINIMUM_BORROW, MAXIMUM_BORROW);

            Console.WriteLine("How much do you plan to deposit");
            //Check for minimum deposit needed
            if (propertyValue < BORROW_THRESHOLD)
            {
                deposit = ReadMinAndMaxDouble($"You need a minimum deposit of {MINIMUM_PERCENT_OF_PROPERTY_VALUE_BELOW_THRESHOLD * 100}% " +
                    $"(£{propertyValue * MINIMUM_PERCENT_OF_PROPERTY_VALUE_BELOW_THRESHOLD}). (Maximum deposit is {MAXIMUM_PERCENT_OF_PROPERTY_VALUE_THRESHOLD * 100}%" +
                    $" of propertyValue (£{propertyValue * MAXIMUM_PERCENT_OF_PROPERTY_VALUE_THRESHOLD}): £",
                    (propertyValue * MINIMUM_PERCENT_OF_PROPERTY_VALUE_BELOW_THRESHOLD), (propertyValue * MAXIMUM_PERCENT_OF_PROPERTY_VALUE_THRESHOLD));
            }
            else
            {
                deposit = ReadMinAndMaxDouble($"You need a minimum deposit of {MINIMUM_PERCENT_OF_PROPERTY_VALUE_ABOVE_THRESHOLD * 100}% " +
                    $"(£{propertyValue * MINIMUM_PERCENT_OF_PROPERTY_VALUE_ABOVE_THRESHOLD }) (Maximum Deposit is {MAXIMUM_PERCENT_OF_PROPERTY_VALUE_THRESHOLD * 100}%" +
                    $" of propertyValue (£{propertyValue * MAXIMUM_PERCENT_OF_PROPERTY_VALUE_THRESHOLD}): £",
                    (propertyValue * MINIMUM_PERCENT_OF_PROPERTY_VALUE_ABOVE_THRESHOLD), (propertyValue * MAXIMUM_PERCENT_OF_PROPERTY_VALUE_THRESHOLD));
            }

            mortgageRepaymentYear = ReadUpperLowerInteger($"How many years do you want your Mortgage for " +
                $"({MINIMUM_MORTGAGE_YEAR} - {MAXIMUM_MORTGAGE_YEAR})? ", MINIMUM_MORTGAGE_YEAR, MAXIMUM_MORTGAGE_YEAR);
            stampDuty = StampDutyCal(propertyValue, STAMP_DUTY_THRESHOLD, STAMPDUTY_LESS_THAN_THRESHOLD, STAMP_DUTY_MORE_THAN_THRESHOLD);
            legalFee = LegalFeeCal(propertyValue, LEGAL_THRESHOLD, LEGAL_FEE_LESS_THAN_THRESHOLD, LEGAL_FEE_MORE_THAN_THRESHOLD);

            toBerepaidAmount = propertyValue - deposit;
            feesPaidUpfront = feesPaidUpfrontCal(stampDuty, legalFee);

            //Mortgage deal constructor 
            MortgageDeal twoYeardeal = new MortgageDeal(FIRST_DEAL_YEAR, FIRST_DEAL_INITIAL_INTEREST, STANDARD_INTEREST, toBerepaidAmount,
                mortgageRepaymentYear, feesPaidUpfront);
            MortgageDeal fiveYearDeal = new MortgageDeal(SECOND_DEAL_YEAR, SECOND_DEAL_INITIAL_INTEREST, STANDARD_INTEREST, toBerepaidAmount,
                mortgageRepaymentYear, feesPaidUpfront);

            //- - - - - - - - - - - - - - - - - - - Printing the deals - - - - - - - -- - - - - - - - - - - - - -- - - - - - -
            Console.WriteLine("\nGreat! These are the deals we have for you. We think you will like one");
            Console.WriteLine("========================================================================");
            twoYeardeal.printDealOutput();
            Console.WriteLine("========================================================================");
            fiveYearDeal.printDealOutput();
            Console.WriteLine("========================================================================");
            printAdditionalCost(stampDuty, legalFee, feesPaidUpfront);

            //Ask User for their deal of choice
            dealOpt1 = ReadStringAorB("\nWill you like to Continue with one of our deal? (Enter Y/N): ", "n", "y");

            if (dealOpt1 == "n")
            {
                choosenDeal = "None";
                customerEmailAddress = "Nil";
                Console.WriteLine("\nWe hope to offer a better deal to suit you next time.\nGOODBYE\n");
            }
            else
            {
                dealOpt2 = ReadStringAorB($"\nChoose a deal (Enter \"A\" for our {FIRST_DEAL_YEAR} years " +
                    $"deal or \"B\" for our {SECOND_DEAL_YEAR} years deal: ", "a", "b");
                Console.WriteLine("\nGreat. Your Choosen deal is:");
                Console.WriteLine("========================================================================");
                if (dealOpt2 == "a")
                {
                    twoYeardeal.printDealOutput();
                    choosenDeal = $"{FIRST_DEAL_YEAR}years";
                }
                else
                {
                    fiveYearDeal.printDealOutput();
                    choosenDeal = $"{SECOND_DEAL_YEAR}years";
                }
                customerEmailAddress = ReadValidEmailAddress("\nPlease provide your Email Addres and we will get back to you: ");
                Console.WriteLine("\nTHANK YOU FOR CHOOSING OLUAPEJ MORTGAGE BANK. We will get back to you soon");
            }

            //Handle exception when the data file to be access is in use or currupt.
            try
            {
                //Reads all user's data and their choices into a csv file for further processing or data analysis.
                userData = string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}{7}", customerName, customerEmailAddress,
                    customerAge, propertyValue, deposit, mortgageRepaymentYear, choosenDeal, newLine);
                File.AppendAllText(userDataFile, userData);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("=============================================================================");
            Console.WriteLine(e.Message);
            Console.WriteLine("The Configuration file data may have been Corrupted or its Setting altered");
            Console.WriteLine("==============================================================================");
        }
    }
}