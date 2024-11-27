using OpenQA.Selenium;
using OpenQA.Selenium.BiDi.Modules.BrowsingContext;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;

internal class Program
{
    private static void Main(string[] args)
    {
        // Define user details
        string firstName = "YourFirstName";
        string lastName = "YourLastName";
        string address = "YourAddress";
        string city = "YourCity";
        string state = "Washington";
        string zipCode = "YourZipCode";
        string phoneNumber = "YourPhoneNumber";
        string email = "YourEmail@example.com";
        string cardNumber = "122387498739817";
        string expirationDate = "09/28"; // Replace with your card expiration date
        string cvc = "123"; // Replace with your card CVC code
        int minTicketsleft = 95;
        string url = "https://mwlpdx.com/event/new-years-eve-celebration-2/";
        using (IWebDriver driver = new ChromeDriver())
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            try
            {
                while (true)
                {
                    // Navigate to the page
                    driver.Navigate().GoToUrl(url);

                    // Check the ticket availability
                    IWebElement quantityElement = wait.Until(d => d.FindElement(By.ClassName("tribe-tickets__item__extra__available__quantity")));
                    int availableQuantity = int.Parse(quantityElement.Text);

                    if (availableQuantity <= minTicketsleft)
                    {
                        Console.WriteLine($"Available tickets: {availableQuantity}. Proceeding...");

                        // Click the add button twice
                        IWebElement addButton = driver.FindElement(By.ClassName("tribe-tickets__item__quantity__add"));
                        addButton.Click();
                        Thread.Sleep(500); // Wait a moment
                        addButton.Click();

                        // Verify quantity is set to 2
                        /* IWebElement quantityDisplay = driver.FindElement(By.ClassName("tribe-common-h3"));
                         if (quantityDisplay.Text != "2")
                         {
                             Console.WriteLine("Quantity is not set to 2. Retrying...");
                             continue;
                         }
                        */
                        // Click the buy button
                        IWebElement buyButton = driver.FindElement(By.Id("tribe-tickets__buy"));
                        buyButton.Click();

                        // Wait for the cart page to load
                        /*wait.Until(d => d.FindElement(By.Id("quantity_67465aa531f3d")));

                        // Verify or update quantity in cart
                        IWebElement cartQuantityField = driver.FindElement(By.Id("quantity_67465aa531f3d"));
                        if (cartQuantityField.GetAttribute("value") != "2")
                        {
                            cartQuantityField.Clear();
                            cartQuantityField.SendKeys("2");

                            // Click update cart
                            IWebElement updateCartButton = driver.FindElement(By.Name("update_cart"));
                            updateCartButton.Click();

                            // Wait for the cart to update
                            Thread.Sleep(1000);
                        }
                        */
                        // Proceed to checkout
                        IWebElement checkoutButton = driver.FindElement(By.ClassName("checkout-button"));
                        checkoutButton.Click();

                        // Wait for the billing details page to load
                        wait.Until(d => d.FindElement(By.Id("billing_first_name")));

                        // Fill in billing details
                        driver.FindElement(By.Id("billing_first_name")).SendKeys(firstName);
                        driver.FindElement(By.Id("billing_last_name")).SendKeys(lastName);
                        driver.FindElement(By.Id("billing_address_1")).SendKeys(address);
                        driver.FindElement(By.Id("billing_city")).SendKeys(city);

                        IWebElement stateDropdown = driver.FindElement(By.Id("select2-billing_state-container"));
                        stateDropdown.Click();
                        IWebElement stateOption = wait.Until(d => d.FindElement(By.XPath("//li[contains(text(), 'Washington')]")));
                        stateOption.Click();

                        driver.FindElement(By.Id("billing_postcode")).SendKeys(zipCode);
                        driver.FindElement(By.Id("billing_phone")).SendKeys(phoneNumber);
                        driver.FindElement(By.Id("billing_email")).SendKeys(email);

                        try
                        {
                            //// Fill in credit card details
                            //// Locate the Stripe iframe
                            //IWebElement stripeIframe = wait.Until(d => d.FindElement(By.CssSelector("iframe[name^='__privateStripeFrame']")));

                            //// Switch to the iframe context
                            //driver.SwitchTo().Frame(stripeIframe);

                            //// Locate the input field within the iframe
                            //wait.Until(d => d.FindElement(By.CssSelector("input[name='cardnumber']"))).SendKeys(cardNumber);

                            //// Locate the iframe for the expiration date field
                            //IWebElement expiryIframe = wait.Until(d => d.FindElement(By.CssSelector("iframe[name^='__privateStripeFrame']")));

                            //// Switch to the iframe
                            //driver.SwitchTo().Frame(expiryIframe);

                            //// Locate the input field within the iframe
                            //IWebElement expiryInput = wait.Until(d => d.FindElement(By.CssSelector("input[name='exp-date']")));

                            //// Send keys for the expiration date (MM/YY format)
                            //expiryInput.SendKeys("12/25");
                            //// Locate the Stripe iframe
                            //IWebElement stripeIframeCVC = wait.Until(d => d.FindElement(By.CssSelector("iframe[name^='__privateStripeFrame3366']")));
                            //driver.SwitchTo().Frame(stripeIframeCVC);
                            //wait.Until(d => d.FindElement(By.CssSelector("input[name='cardCvc']"))).SendKeys(cvc);

                            // Function to interact with iframe input fields
                            void SendKeysToIframe(string iframeSelector, string inputSelector, string keysToSend)
                            {
                                IWebElement iframe = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(iframeSelector)));
                                driver.SwitchTo().Frame(iframe);
                                IWebElement inputField = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(inputSelector)));
                                inputField.SendKeys(keysToSend);
                                driver.SwitchTo().DefaultContent();
                            }

                            // Send keys to Card Number field
                            SendKeysToIframe("iframe[src*='cardNumber']", "input", "4242424242424242");

                            // Send keys to Expiry Date field
                            SendKeysToIframe("iframe[src*='cardExpiry']", "input", "12/25");

                            // Send keys to CVC field
                            SendKeysToIframe("iframe[src*='cardCvc']", "input", "123");

                            Console.WriteLine("All fields filled successfully.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"An error occurred: {ex.Message}");
                        }
                        // Place the order
                        IWebElement placeOrderButton = driver.FindElement(By.Id("place_order"));
                        //placeOrderButton.Click();

                        Console.WriteLine("Order placed successfully!");
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"Available tickets: {availableQuantity}. Retrying in 10 seconds...");
                        Thread.Sleep(10); // Wait x seconds before checking again
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
}