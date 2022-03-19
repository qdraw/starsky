using Microsoft.VisualStudio.TestTools.UnitTesting;
using starsky.foundation.readmeta.Helpers;

namespace starskytest.starsky.foundation.readmeta.Helpers
{
	[TestClass]
	public class MathFractionTest
	{
		[TestMethod]
		public void MathFraction_Fraction1()
		{
			var fraction = MathFraction.Fraction("1/1");
			Assert.AreEqual(1,fraction,0.00001);
		}
		
		[TestMethod]
		public void MathFraction_Fraction78()
		{
			var fraction = MathFraction.Fraction("7/8");
			Assert.AreEqual(0.875,fraction,0.00001);
		}

		[TestMethod]
		public void MathFraction_FractionNotRight()
		{
			var fraction = MathFraction.Fraction("7/8/0");
			Assert.AreEqual(0,fraction,0.00001);
		}
	}
}
