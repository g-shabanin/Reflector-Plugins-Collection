cd ..\Source
%SystemRoot%\Microsoft.net\Framework\v3.5\MSBuild.exe /target:Clean
%SystemRoot%\Microsoft.net\Framework\v3.5\MSBuild.exe /target:Build

cd ..\Tools

if exist ..\Deploy rd /S /Q ..\Build\Deploy
md ..\Build\Deploy

Zip.exe ..\Build\Deploy\ClassView.zip ..\Build Reflector.ClassView.dll
Zip.exe ..\Build\Deploy\CodeModelViewer.zip ..\Build Reflector.CodeModelViewer.dll
Zip.exe ..\Build\Deploy\ComLoader.zip ..\Build Reflector.ComLoader.dll
Zip.exe ..\Build\Deploy\Graph.zip ..\Build Reflector.Graph.dll QuickGraph.dll QuickGraph.Algorithms.dll Microsoft.GLEE.dll Microsoft.GLEE.Drawing.dll Microsoft.GLEE.GraphViewerGDI.dll "GLEE SHARED SOURCE LICENSE 2006-08-22.rtf" Reflector.exe.config
Zip.exe ..\Build\Deploy\CodeMetrics.zip ..\Build Reflector.CodeMetrics.dll TreemapControl.dll TreemapGenerator.dll Reflector.exe.config
Zip.exe ..\Build\Deploy\CodeSearch.zip ..\Build Reflector.CodeSearch.dll
Zip.exe ..\Build\Deploy\BizTalkDisassembler.zip ..\Build Reflector.BizTalkDisassembler.dll Reflector.exe.config
Zip.exe ..\Build\Deploy\DelphiLanguage.zip ..\Build Reflector.DelphiLanguage.dll
Zip.exe ..\Build\Deploy\MCppLanguage.zip ..\Build Reflector.MCppLanguage.dll
Zip.exe ..\Build\Deploy\Review.zip ..\Build Reflector.Review.dll
Zip.exe ..\Build\Deploy\BamlViewer.zip ..\Build Reflector.BamlViewer.dll
Zip.exe ..\Build\Deploy\PowerShellLanguage.zip ..\Build Reflector.PowerShellLanguage.dll
Zip.exe ..\Build\Deploy\SilverlightLoader.zip ..\Build Reflector.SilverlightLoader.dll
Zip.exe ..\Build\Deploy\SilverlightBrowser.zip ..\Build Reflector.SilverlightBrowser.dll Reflector.exe.config
Zip.exe ..\Build\Deploy\ReflectionEmitLanguage.zip ..\Build Reflector.ReflectionEmitLanguage.dll
Zip.exe ..\Build\Deploy\RuleSetEditor.zip ..\Build Reflector.RuleSetEditor.dll
Zip.exe ..\Build\Deploy\VulcanLanguage.zip ..\Build Reflector.VulcanLanguage.dll
Zip.exe ..\Build\Deploy\UmlExporter.zip ..\Build Reflector.UmlExporter.dll
