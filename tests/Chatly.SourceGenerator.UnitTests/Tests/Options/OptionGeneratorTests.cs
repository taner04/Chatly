using Chatly.SourceGenerator.UnitTests.Tests.Options.Infrastructure;

namespace Chatly.SourceGenerator.UnitTests.Tests.Options;

public sealed class OptionGeneratorTests
{
    [Fact]
    public void Generator_Should_BindSectionNamedAfterClass_When_NoNameIsGiven()
    {
        var generated = OptionGeneratorTestHelper.Run("""
                                                      namespace App;

                                                      [Chatly.Shared.Attributes.Option]
                                                      public sealed class MailOption;
                                                      """);

        generated.Should().Contain("AddOptions<global::App.MailOption>(services)");
        generated.Should().Contain(".BindConfiguration(\"MailOption\")");
        generated.Should().Contain(".ValidateDataAnnotations()");
        generated.Should().Contain(".ValidateOnStart();");
    }

    [Fact]
    public void Generator_Should_UseConfiguredSectionName_When_NameIsGiven()
    {
        var generated = OptionGeneratorTestHelper.Run("""
                                                      namespace App;

                                                      [Chatly.Shared.Attributes.Option("Mail")]
                                                      public sealed class MailOption;
                                                      """);

        generated.Should().Contain(".BindConfiguration(\"Mail\")");
        generated.Should().NotContain(".BindConfiguration(\"MailOption\")");
    }

    [Fact]
    public void Generator_Should_RegisterEveryOptionInStableOrder_When_SeveralOptionsExist()
    {
        var generated = OptionGeneratorTestHelper.Run("""
                                                      namespace App;

                                                      [Chatly.Shared.Attributes.Option]
                                                      public sealed class ZetaOption;

                                                      [Chatly.Shared.Attributes.Option]
                                                      public sealed class AlphaOption;
                                                      """);

        generated.IndexOf("AlphaOption", StringComparison.Ordinal)
            .Should().BeLessThan(generated.IndexOf("ZetaOption", StringComparison.Ordinal));
    }

    [Fact]
    public void Generator_Should_EmitEmptyRegistration_When_NoOptionsExist()
    {
        var generated = OptionGeneratorTestHelper.Run("namespace App; public sealed class Plain;");

        generated.Should().Contain("AddGeneratedOptions(");
        generated.Should().NotContain("BindConfiguration");
    }
}
