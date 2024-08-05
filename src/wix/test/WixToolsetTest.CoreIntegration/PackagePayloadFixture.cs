// Copyright (c) .NET Foundation and contributors. All rights reserved. Licensed under the Microsoft Reciprocal License. See LICENSE.TXT file in the project root for full license information.

namespace WixToolsetTest.CoreIntegration
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using WixInternal.TestSupport;
    using WixInternal.Core.TestPackage;
    using Xunit;

    public class PackagePayloadFixture
    {
        [Fact]
        public void CanSpecifyMsiPackagePayloadInPayloadGroup()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var bundlePath = Path.Combine(baseFolder, @"bin\test.exe");
                var baFolderPath = Path.Combine(baseFolder, "ba");
                var extractFolderPath = Path.Combine(baseFolder, "extract");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "PackagePayload", "MsiPackagePayloadInPayloadGroup.wxs"),
                    Path.Combine(folder, "BundleWithPackageGroupRef", "Bundle.wxs"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-bindpath", Path.Combine(folder, ".Data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", bundlePath,
                });

                result.AssertSuccess();

                Assert.True(File.Exists(bundlePath));

                var extractResult = BundleExtractor.ExtractBAContainer(null, bundlePath, baFolderPath, extractFolderPath);
                extractResult.AssertSuccess();

                var ignoreAttributesByElementName = new Dictionary<string, List<string>>
                {
                    { "ExePackage", new List<string> { "CacheId", "InstallSize", "Size" } },
                };
                var msiPackageElements = extractResult.GetManifestTestXmlLines("/burn:BurnManifest/burn:Chain/burn:MsiPackage", ignoreAttributesByElementName);
                WixAssert.CompareLineByLine(new[]
                {
                    "<MsiPackage Id='MsiWithFeatures' Cache='keep' CacheId='{040011E1-F84C-4927-AD62-50A5EC19CA32}v1.0.0.0_1' InstallSize='34' Size='32803' PerMachine='yes' Permanent='no' Vital='yes' RollbackBoundaryForward='WixDefaultBoundary' LogPathVariable='WixBundleLog_MsiWithFeatures' RollbackLogPathVariable='WixBundleRollbackLog_MsiWithFeatures' ProductCode='{040011E1-F84C-4927-AD62-50A5EC19CA32}' Language='1033' Version='1.0.0.0' UpgradeCode='{047730A5-30FE-4A62-A520-DA9381B8226A}'>" +
                    "<MsiFeature Id='ProductFeature' /><MsiProperty Id='ARPSYSTEMCOMPONENT' Value='1' /><MsiProperty Id='MSIFASTINSTALL' Value='7' /><Provides Key='{040011E1-F84C-4927-AD62-50A5EC19CA32}_v1.0.0.0' Version='1.0.0.0' DisplayName='MsiPackage' />" +
                    "<RelatedPackage Id='{047730A5-30FE-4A62-A520-DA9381B8226A}' MaxVersion='1.0.0.0' MaxInclusive='no' OnlyDetect='no' LangInclusive='yes'><Language Id='1033' /></RelatedPackage>" +
                    "<RelatedPackage Id='{047730A5-30FE-4A62-A520-DA9381B8226A}' MinVersion='1.0.0.0' MinInclusive='no' OnlyDetect='yes' LangInclusive='yes'><Language Id='1033' /></RelatedPackage>" +
                    "<PayloadRef Id='test.msi' /><PayloadRef Id='fhuZsOcBDTuIX8rF96kswqI6SnuI' /><PayloadRef Id='faf_OZ741BG7SJ6ZkcIvivZ2Yzo8' />" +
                    "</MsiPackage>",

                    "<MsiPackage Id='MsiWithoutFeatures' Cache='keep' CacheId='{040011E1-F84C-4927-AD62-50A5EC19CA32}v1.0.0.0_2' InstallSize='34' Size='32803' PerMachine='yes' Permanent='no' Vital='yes' RollbackBoundaryBackward='WixDefaultBoundary' LogPathVariable='WixBundleLog_MsiWithoutFeatures' RollbackLogPathVariable='WixBundleRollbackLog_MsiWithoutFeatures' ProductCode='{040011E1-F84C-4927-AD62-50A5EC19CA32}' Language='1033' Version='1.0.0.0' UpgradeCode='{047730A5-30FE-4A62-A520-DA9381B8226A}'>" +
                    "<MsiProperty Id='ARPSYSTEMCOMPONENT' Value='1' /><MsiProperty Id='MSIFASTINSTALL' Value='7' /><Provides Key='{040011E1-F84C-4927-AD62-50A5EC19CA32}_v1.0.0.0' Version='1.0.0.0' DisplayName='MsiPackage' />" +
                    "<RelatedPackage Id='{047730A5-30FE-4A62-A520-DA9381B8226A}' MaxVersion='1.0.0.0' MaxInclusive='no' OnlyDetect='no' LangInclusive='yes'><Language Id='1033' /></RelatedPackage>" +
                    "<RelatedPackage Id='{047730A5-30FE-4A62-A520-DA9381B8226A}' MinVersion='1.0.0.0' MinInclusive='no' OnlyDetect='yes' LangInclusive='yes'><Language Id='1033' /></RelatedPackage>" +
                    "<PayloadRef Id='test.msi' /><PayloadRef Id='fhuZsOcBDTuIX8rF96kswqI6SnuI' /><PayloadRef Id='faf_OZ741BG7SJ6ZkcIvivZ2Yzo8' />" +
                    "</MsiPackage>",
                }, msiPackageElements);

                var packageElements = extractResult.GetBADataTestXmlLines("/ba:BootstrapperApplicationData/ba:WixPackageProperties");
                WixAssert.CompareLineByLine(new[]
                {
                    "<WixPackageProperties Package='MsiWithFeatures' Vital='yes' DisplayName='MsiPackage' DownloadSize='32803' PackageSize='32803' InstalledSize='34' PackageType='Msi' Permanent='no' LogPathVariable='WixBundleLog_MsiWithFeatures' RollbackLogPathVariable='WixBundleRollbackLog_MsiWithFeatures' Compressed='yes' ProductCode='{040011E1-F84C-4927-AD62-50A5EC19CA32}' UpgradeCode='{047730A5-30FE-4A62-A520-DA9381B8226A}' Version='1.0.0.0' Cache='keep' />",
                    "<WixPackageProperties Package='MsiWithoutFeatures' Vital='yes' DisplayName='MsiPackage' DownloadSize='32803' PackageSize='32803' InstalledSize='34' PackageType='Msi' Permanent='no' LogPathVariable='WixBundleLog_MsiWithoutFeatures' RollbackLogPathVariable='WixBundleRollbackLog_MsiWithoutFeatures' Compressed='yes' ProductCode='{040011E1-F84C-4927-AD62-50A5EC19CA32}' UpgradeCode='{047730A5-30FE-4A62-A520-DA9381B8226A}' Version='1.0.0.0' Cache='keep' />",
                }, packageElements);

                var featureElements = extractResult.GetBADataTestXmlLines("/ba:BootstrapperApplicationData/ba:WixPackageFeatureInfo");
                WixAssert.CompareLineByLine(new[]
                {
                    "<WixPackageFeatureInfo Package='MsiWithFeatures' Feature='ProductFeature' Size='34' Display='2' Level='1' Directory='' Attributes='0' />",
                }, featureElements);
            }
        }

        [Fact(Skip = "Depends on a v5 extension being available, which isn't true for nuget.org yet or this early in the build.")]
        public void CanSpecifyExePackagePayloadInPayloadGroup()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var bundlePath = Path.Combine(baseFolder, @"bin\test.exe");
                var baFolderPath = Path.Combine(baseFolder, "ba");
                var extractFolderPath = Path.Combine(baseFolder, "extract");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "PackagePayload", "PackagePayloadInPayloadGroup.wxs"),
                    Path.Combine(folder, "BundleWithPackageGroupRef", "Bundle.wxs"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-bindpath", Path.Combine(folder, ".Data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", bundlePath,
                });

                result.AssertSuccess();

                Assert.True(File.Exists(bundlePath));

                var extractResult = BundleExtractor.ExtractBAContainer(null, bundlePath, baFolderPath, extractFolderPath);
                extractResult.AssertSuccess();

                var ignoreAttributesByElementName = new Dictionary<string, List<string>>
                {
                    { "ExePackage", new List<string> { "CacheId", "InstallSize", "Size" } },
                };
                var exePackageElements = extractResult.GetManifestTestXmlLines("/burn:BurnManifest/burn:Chain/burn:ExePackage", ignoreAttributesByElementName);
                WixAssert.CompareLineByLine(new[]
                {
                    "<ExePackage Id='PackagePayloadInPayloadGroup' Cache='keep' CacheId='*' InstallSize='*' Size='*' PerMachine='yes' Permanent='yes' Vital='yes' RollbackBoundaryForward='WixDefaultBoundary' RollbackBoundaryBackward='WixDefaultBoundary' LogPathVariable='WixBundleLog_PackagePayloadInPayloadGroup' RollbackLogPathVariable='WixBundleRollbackLog_PackagePayloadInPayloadGroup' InstallArguments='' RepairArguments='' Repairable='no' DetectionType='condition' DetectCondition='none'><PayloadRef Id='burn.exe' /></ExePackage>",
                }, exePackageElements);

                var payloadElements = extractResult.GetManifestTestXmlLines("/burn:BurnManifest/burn:Payload[@Id='burn.exe']");
                WixAssert.CompareLineByLine(new[]
                {
                    "<Payload Id='burn.exe' FilePath='burn.exe' FileSize='463360' Hash='F6E722518AC3AB7E31C70099368D5770788C179AA23226110DCF07319B1E1964E246A1E8AE72E2CF23E0138AFC281BAFDE45969204405E114EB20C8195DA7E5E' Packaging='embedded' SourcePath='a0' Container='WixAttachedContainer' />",
                }, payloadElements);
            }
        }

        [Fact]
        public void CanSpecifyExePackagePayloadWithCertificate()
        {
            var folder = TestData.Get(@"TestData", "PackagePayload");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "SpecifiedCertificate.wxs"),
                    "-o", Path.Combine(baseFolder, "test.wixlib")
                });

                result.AssertSuccess();
            }
        }

        [Fact]
        public void ErrorWhenMissingSourceFileAndHash()
        {
            var folder = TestData.Get(@"TestData", "PackagePayload");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();

                var result = WixRunner.Execute(false, new[]
                {
                    "build",
                    Path.Combine(folder, "MissingSourceFileAndHash.wxs"),
                    "-o", Path.Combine(baseFolder, "test.wixlib")
                });

                Assert.Equal(44, result.ExitCode);
                WixAssert.CompareLineByLine(new[]
                {
                    "The MsuPackagePayload element's SourceFile, CertificatePublicKey, or Hash attribute was not found; one of these is required.",
                }, result.Messages.Select(m => m.ToString()).ToArray());
            }
        }

        [Fact]
        public void ErrorWhenMissingSourceFileAndName()
        {
            var folder = TestData.Get(@"TestData", "PackagePayload");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();

                var result = WixRunner.Execute(false, new[]
                {
                    "build",
                    Path.Combine(folder, "MissingSourceFileAndName.wxs"),
                    "-o", Path.Combine(baseFolder, "test.wixlib")
                });

                Assert.Equal(44, result.ExitCode);
                WixAssert.CompareLineByLine(new[]
                {
                    "The MsiPackagePayload element's Name or SourceFile attribute was not found; one of these is required.",
                }, result.Messages.Select(m => m.ToString()).ToArray());
            }
        }

        [Fact]
        public void ErrorWhenSpecifiedHash()
        {
            var folder = TestData.Get(@"TestData", "PackagePayload");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "SpecifiedHash.wxs"),
                    "-o", Path.Combine(baseFolder, "test.wixlib")
                });

                Assert.Equal(4, result.ExitCode);
                WixAssert.CompareLineByLine(new[]
                {
                    "The MspPackagePayload element contains an unexpected attribute 'Hash'.",
                }, result.Messages.Select(m => m.ToString()).ToArray());
            }
        }

        [Fact]
        public void ErrorWhenSpecifiedHashAndMissingDownloadUrl()
        {
            var folder = TestData.Get(@"TestData", "PackagePayload");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "SpecifiedHashAndMissingDownloadUrl.wxs"),
                    "-o", Path.Combine(baseFolder, "test.wixlib")
                });

                Assert.Equal(408, result.ExitCode);
                WixAssert.CompareLineByLine(new[]
                {
                    "The MsuPackagePayload element's DownloadUrl attribute was not found; it is required without attribute SourceFile present.",
                }, result.Messages.Select(m => m.ToString()).ToArray());
            }
        }

        [Fact]
        public void ErrorWhenSpecifiedSourceFileAndHash()
        {
            var folder = TestData.Get(@"TestData", "PackagePayload");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "SpecifiedSourceFileAndHash.wxs"),
                    "-o", Path.Combine(baseFolder, "test.wixlib")
                });

                Assert.Equal(35, result.ExitCode);
                WixAssert.CompareLineByLine(new[]
                {
                    "The ExePackagePayload/@Hash attribute cannot be specified when attribute SourceFile is present.",
                }, result.Messages.Select(m => m.ToString()).ToArray());
            }
        }

        [Fact]
        public void ErrorWhenSpecifiedSourceFileAndCertificatePublicKey()
        {
            var folder = TestData.Get(@"TestData", "PackagePayload");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "SpecifiedSourceFileAndCertificatePublicKey.wxs"),
                    "-o", Path.Combine(baseFolder, "test.wixlib")
                });

                WixAssert.CompareLineByLine(new[]
                {
                    "The ExePackagePayload/@CertificatePublicKey attribute cannot be specified when attribute SourceFile is present.",
                }, result.Messages.Select(m => m.ToString()).ToArray());
                Assert.Equal(35, result.ExitCode);
            }
        }

        [Fact]
        public void ErrorWhenSpecifiedSourceFileAndCertificateThumprint()
        {
            var folder = TestData.Get(@"TestData", "PackagePayload");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "SpecifiedSourceFileAndCertificateThumbprint.wxs"),
                    "-o", Path.Combine(baseFolder, "test.wixlib")
                });

                WixAssert.CompareLineByLine(new[]
                {
                    "The ExePackagePayload/@CertificateThumbprint attribute cannot be specified when attribute SourceFile is present.",
                }, result.Messages.Select(m => m.ToString()).ToArray());
                Assert.Equal(35, result.ExitCode);
            }
        }

        [Fact]
        public void ErrorWhenSpecifiedCertificateThumbprintWithoutPublicKey()
        {
            var folder = TestData.Get(@"TestData", "PackagePayload");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "SpecifiedHashAndCertificatePublicKey.wxs"),
                    "-o", Path.Combine(baseFolder, "test.wixlib")
                });

                WixAssert.CompareLineByLine(new[]
                {
                    "The ExePackagePayload/@CertificatePublicKey attribute was not found; it is required when attribute CertificateThumbprint is specified.",
                    "The ExePackagePayload/@Hash attribute cannot be specified when attribute CertificateThumbprint is present."
                }, result.Messages.Select(m => m.ToString()).ToArray());
                Assert.Equal(35, result.ExitCode);
            }
        }

        [Fact]
        public void ErrorWhenSpecifiedCertificatePublicKeyWithoutThumbprint()
        {
            var folder = TestData.Get(@"TestData", "PackagePayload");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "SpecifiedCertificatePublicKeyWithoutThumbprint.wxs"),
                    "-o", Path.Combine(baseFolder, "test.wixlib")
                });

                WixAssert.CompareLineByLine(new[]
                {
                    "The ExePackagePayload/@CertificateThumbprint attribute was not found; it is required when attribute CertificatePublicKey is specified.",
                }, result.Messages.Select(m => m.ToString()).ToArray());
                Assert.Equal(10, result.ExitCode);
            }
        }

        [Fact]
        public void ErrorWhenWrongPackagePayloadInPayloadGroup()
        {
            var folder = TestData.Get(@"TestData");

            using (var fs = new DisposableFileSystem())
            {
                var baseFolder = fs.GetFolder();
                var intermediateFolder = Path.Combine(baseFolder, "obj");
                var bundlePath = Path.Combine(baseFolder, @"bin\test.exe");

                var result = WixRunner.Execute(new[]
                {
                    "build",
                    Path.Combine(folder, "PackagePayload", "WrongPackagePayloadInPayloadGroup.wxs"),
                    Path.Combine(folder, "BundleWithPackageGroupRef", "Bundle.wxs"),
                    "-bindpath", Path.Combine(folder, "SimpleBundle", "data"),
                    "-bindpath", Path.Combine(folder, ".Data"),
                    "-intermediateFolder", intermediateFolder,
                    "-o", bundlePath,
                });

                Assert.Equal(407, result.ExitCode);
                WixAssert.CompareLineByLine(new[]
                {
                    "The ExePackagePayload element can only be used for ExePackages.",
                    "The location of the package related to previous error.",
                    "There is no payload defined for package 'WrongPackagePayloadInPayloadGroup'. This is specified on the MsiPackage element or a child MsiPackagePayload element.",
                }, result.Messages.Select(m => m.ToString()).ToArray());
            }
        }
    }
}
