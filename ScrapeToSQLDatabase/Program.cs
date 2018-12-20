﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Data.SqlClient;

namespace ScrapeToSQLDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Scraping started.");
            // Add (--headless) to chromeoptions to block window from popping up
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("window-size=1920,1080");


            // Initiate new ChromeDriver called driver and navigate to login URL
            IWebDriver driver = new ChromeDriver(options);

            driver.Navigate().GoToUrl("https://login.yahoo.com/config/login?.src=finance&.intl=us&.done=https%3A%2F%2Ffinance.yahoo.com%2F");
            driver.Manage().Window.Maximize();

            // Input username field, submit
            IWebElement username = driver.FindElement(By.Name("username"));
            username.SendKeys("mikeishere3@intracitygeeks.org");
            username.Submit();

            // Initiate new driver wait to wait for page to load and elements to appear
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));

            // Wait, then Input password field, submit using click
            IWebElement password = wait.Until(d => driver.FindElement(By.Id("login-passwd")));
            password.SendKeys("scraper123");
            driver.FindElement(By.Id("login-signin")).Click();

            // Navigate to portfolio url once logged in
            driver.Navigate().GoToUrl("https://finance.yahoo.com/portfolio/p_0/view/v1");

            // Wait for tr elements to load
            wait.Until(d => driver.FindElement(By.TagName("tbody")));

            // Get a count of the stocks in the list
            int count;
            IWebElement list = driver.FindElement(By.TagName("tbody"));
            System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> items = list.FindElements(By.TagName("tr"));
            count = items.Count;

            //Console.WriteLine("\nThere are " + count + " stocks in the list\n");

            // Create list to store stocks
            List<Stock> stockList = new List<Stock>();

            //Loop to iterate through names and prices of stocks
            for (int i = 1; i <= count; i++)
            {
                var symbol = driver.FindElement(By.XPath("//*[@id=\"main\"]/section/section[2]/div[2]/table/tbody/tr[" + i + "]/td[1]/span/a")).GetAttribute("innerText");
                var price = driver.FindElement(By.XPath("//*[@id=\"main\"]/section/section[2]/div[2]/table/tbody/tr[" + i + "]/td[2]/span")).GetAttribute("innerText");
                var change = driver.FindElement(By.XPath("//*[@id=\"main\"]/section/section[2]/div[2]/table/tbody/tr[" + i + "]/td[3]/span")).GetAttribute("innerText");
                var pchange = driver.FindElement(By.XPath("//*[@id=\"main\"]/section/section[2]/div[2]/table/tbody/tr[" + i + "]/td[4]/span")).GetAttribute("innerText");
                var volume = driver.FindElement(By.XPath("//*[@id=\"main\"]/section/section[2]/div[2]/table/tbody/tr[" + i + "]/td[7]/span")).GetAttribute("innerText");
                var marketcap = driver.FindElement(By.XPath("//*[@id=\"main\"]/section/section[2]/div[2]/table/tbody/tr[" + i + "]/td[13]/span")).GetAttribute("innerText");

                Stock newStock = new Stock();
                newStock.Symbol = symbol;
                newStock.Price = price;
                newStock.Change = change;
                newStock.PChange = pchange;
                newStock.Volume = volume;
                newStock.MarketCap = marketcap;

                stockList.Add(newStock);
            }
            
            driver.Quit();

            Console.WriteLine("Scraping done.");

            string connString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=NewStockDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            SqlConnection conn = new SqlConnection(connString);
            conn.Open();

            foreach (Stock stock in stockList)
            {
                SqlCommand insertCommand = new SqlCommand("INSERT INTO [Stock] (Symbol, Price, Change, PChange, Volume, MarketCap) VALUES (@symbol, @price, @change, @pchange, @volume, @marketcap)", conn);
                insertCommand.Parameters.AddWithValue("symbol", stock.Symbol);
                insertCommand.Parameters.AddWithValue("price", stock.Price);
                insertCommand.Parameters.AddWithValue("change", stock.Change);
                insertCommand.Parameters.AddWithValue("pchange", stock.PChange);
                insertCommand.Parameters.AddWithValue("volume", stock.Volume);
                insertCommand.Parameters.AddWithValue("marketcap", stock.MarketCap);
                insertCommand.ExecuteNonQuery();
            }

            conn.Close();
            Console.WriteLine("Database updated.");
        }
    }
}
