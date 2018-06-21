using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.Models;
using starsky.ViewModels;

namespace starskytests
{

    [TestClass]
    public class EnvViewModelTest
    {
        [TestMethod]
        public void EnvViewModelTestEnvViewModelTest()
        {
            new EnvViewModel().GetEnvAppSettingsProvider();
        }

    }
}