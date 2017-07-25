using Sitecore.Analytics.Data.Items;
using Sitecore.Analytics.Shell.Applications.WebEdit;
using Sitecore.Analytics.Testing.TestingUtils;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Mvc.Analytics.Pipelines.Response.CustomizeRendering;
using Sitecore.Web;
using System;
using System.Linq;

namespace Sitecore.Support.Mvc.ExperienceEditor.Pipelines.Response.CustomizeRendering
{
    public class SelectVariation : Sitecore.Mvc.Analytics.Pipelines.Response.CustomizeRendering.SelectVariation
    {
        protected override MultivariateTestValueItem GetVariation(Item variableItem, Item contextItem, ID deviceId)
        {
            Assert.ArgumentNotNull(variableItem, "variableItem");
            MultivariateTestVariableItem multivariateTestVariableItem = (MultivariateTestVariableItem)variableItem;
            MultivariateTestValueItem result;
            if (multivariateTestVariableItem == null)
            {
                result = null;
            }
            else
            {
                MultivariateTestDefinitionItem multivariateTestDefinitionItem = (MultivariateTestDefinitionItem)variableItem.Parent;
                if (multivariateTestDefinitionItem != null)
                {
                    SelectVariation.UpdateTestSettings(multivariateTestDefinitionItem);
                }
                string queryString = WebUtil.GetQueryString("sc_combination");
                try
                {
                    if (!string.IsNullOrEmpty(queryString))
                    {
                        int num = -1;
                        if (int.TryParse(queryString, out num))
                        {
                            MultivariateTestValueItem[] array = TestingUtil.MultiVariateTesting.GetVariableValues(multivariateTestVariableItem).ToArray<MultivariateTestValueItem>();
                            MultivariateTestValueItem multivariateTestValueItem = array[num];
                            if (multivariateTestValueItem != null)
                            {
                                result = multivariateTestValueItem;
                                return result;
                            }
                        }
                    }
                }
                catch
                {
                }
                result = TestingUtil.MultiVariateTesting.GetVariableValues(multivariateTestVariableItem).LastOrDefault<MultivariateTestValueItem>();
            }
            return result;
        }

        public override void Process(CustomizeRenderingArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (!args.IsCustomized && Context.PageMode.IsPageEditor && Settings.Analytics.Enabled)
            {
                this.Evaluate(args);
            }
        }

        private static void UpdateTestSettings(MultivariateTestDefinitionItem testDefinition)
        {
            Assert.ArgumentNotNull(testDefinition, "testDefinition");
            if (WebEditUtil.Testing.CurrentSettings == null)
            {
                bool flag = TestingUtil.MultiVariateTesting.IsTestRunning(testDefinition);
                WebEditUtil.Testing.TestSettings currentSettings = new WebEditUtil.Testing.TestSettings(testDefinition, Sitecore.Web.WebEditUtil.Testing.TestType.Multivariate, flag);
                WebEditUtil.Testing.CurrentSettings = currentSettings;
                if (flag)
                {
                    TestDefinitionItem testDefinitionItem = testDefinition;
                    Assert.IsNotNull(testDefinitionItem, "testDefinitionItem");
                    PageStatisticsContext.SaveTestStatisticsToSession(PageStatisticsContext.GetTestStatistics(testDefinitionItem, true, false));
                }
            }
        }
    }
}
