using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using WorkshopTest.ComponentHelper;
using WorkshopTest.Configuration;

namespace BDD_Snake.Steps;

[Binding]
public sealed class CreditCardValidatorStepDefinitions
{
    // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

    private readonly ScenarioContext _scenarioContext;
    private IWebElement creditCardNumberTextBox { get; set; }
    private IWebElement expirationDateTextBox { get; set; }
    private IWebElement CVCTextBox { get; set; }
    
    private readonly ConfigReader _config = new ConfigReader();

    public CreditCardValidatorStepDefinitions(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given(@"user fills the three inputs")]
    public void GivenUserFillsTheThreeInputs()
    {
        creditCardNumberTextBox = GenericHelper.GetElement(By.Id("creditCardNumber"));
        expirationDateTextBox = GenericHelper.GetElement(By.Id("expirationDate"));
        CVCTextBox = GenericHelper.GetElement(By.Id("cvc"));
        
        Assert.IsNotNull(creditCardNumberTextBox);
        Assert.IsNotNull(expirationDateTextBox);
        Assert.IsNotNull(CVCTextBox);
        
        creditCardNumberTextBox.SendKeys(_config.GetCardNumber());
        expirationDateTextBox.SendKeys(_config.GetExpirationDate());
        CVCTextBox.SendKeys(_config.GetCvc());
    }

    [Given(@"credit card number is sixteen digits long")]
    public void GivenCreditCardNumberIsSixteenDigitsLong()
    {
        string creditCardNumber = creditCardNumberTextBox.GetAttribute("value");
        Assert.AreEqual(16, creditCardNumber.Length);
        Assert.IsTrue(long.TryParse(creditCardNumber, out long creditCard));
    }

    [Given(@"expiration date is at format MM/YYYY")]
    public void GivenExpirationDateIsAtFormatMmyyyy()
    {
        string expirationDate = expirationDateTextBox.GetAttribute("value");
        
        Assert.AreEqual(expirationDate.Length, 7);
        Assert.AreEqual(expirationDate.Substring(2, 1), "/");
        Assert.IsTrue(int.TryParse(expirationDate.Substring(0, 2), out int month));
        Assert.IsTrue(int.TryParse(expirationDate.Substring(3, 4), out int year));
        Assert.IsTrue(month >= 1 && month <= 12);
        Assert.IsTrue(year >= 1987 && year <= 2099);
    }

    [Given(@"cvc is three digits long")]
    public void GivenCvcIsThreeDigitsLong()
    {
        string cvc = CVCTextBox.GetAttribute("value");
        Assert.AreEqual(cvc.Length, 3);
        Assert.IsTrue(int.TryParse(cvc, out int value));
        Assert.IsTrue(value >= 100 && value <= 999);
    }

    [Given(@"cvc is not three digits long")]
    public void GivenCvcIsNotThreeDigitsLong()
    {
        CVCTextBox.Clear();
        CVCTextBox.SendKeys("12");
        string cvc = CVCTextBox.GetAttribute("value");
        
        Assert.AreNotEqual(cvc.Length, 3);
    }

    [When(@"submit button is pressed")]
    public void WhenSubmitButtonIsPressed()
    {
        GenericHelper.GetElement(By.Id("submitCard")).Click();
    }

    [Then(@"user is on page paymentConfirmed")]
    public void ThenUserIsOnPagePaymentConfirmed()
    {
        Assert.IsTrue(GenericHelper.GetElement(By.Id("page-title")).Displayed);
    }

    [Given(@"credit card number is not sixteen digits long")]
    public void GivenCreditCardNumberIsNotSixteenDigitsLong()
    {
        creditCardNumberTextBox.Clear();
        creditCardNumberTextBox.SendKeys("41111");
        
        string creditCardNumber = creditCardNumberTextBox.GetAttribute("value");
        
        Assert.AreNotEqual(creditCardNumber.Length, 16);
    }

    [Then(@"user is on homePage")]
    public void ThenUserIsOnHomePage()
    {
        Assert.IsTrue(PageHelper.GetPageUrl()!.Contains("Workshop.html"));
    }

    [Given(@"expiration date is not at format MM/YYYY")]
    public void GivenExpirationDateIsNotAtFormatMmyyyy()
    {
        expirationDateTextBox.Clear();
        expirationDateTextBox.SendKeys("1-21");
        
        string expirationDate = expirationDateTextBox.GetAttribute("value");
        
        Assert.AreNotEqual(expirationDate.Length, 7);
        Assert.AreNotEqual(expirationDate.Substring(2, 1), "/");
    }
}