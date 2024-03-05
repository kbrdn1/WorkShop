using System;
using TechTalk.SpecFlow;
using WorkshopTest.BaseClasses;
using WorkshopTest.ComponentHelper;
using WorkshopTest.Configuration;
using WorkshopTest.Settings;

namespace BDD_Snake.Hooks
{
    [Binding]
    public class InitScenarioHook
    {
        [BeforeScenario]
        public static void InitScenario()
        {
            ObjectRepository.Config = new ConfigReader();

            switch (ObjectRepository.Config.GetBrowser())
            {
                case BrowserType.Chrome:
                    ObjectRepository.Driver = BaseClass.GetChromeWebDriver();
                    break;

                case BrowserType.Firefox:
                    ObjectRepository.Driver = BaseClass.GetFirefoxWebDriver();
                    break;

                case BrowserType.InternetExplorer:
                    ObjectRepository.Driver = BaseClass.GetInternetExplorerWebDriver();
                    break;
            }

            NavigationHelper.NavigateToUrl(ObjectRepository.Config.GetWebsite() + "Workshop.html");
        }

        [AfterScenario]
        public static void TearDown()
        {
            if (ObjectRepository.Driver != null)
            {
                ObjectRepository.Driver.Close();
                ObjectRepository.Driver.Quit();
            }
        }
    }
}