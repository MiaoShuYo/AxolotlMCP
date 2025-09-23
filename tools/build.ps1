# AxolotlMCP 构建脚本
param(
    [string]$Configuration = "Release",
    [string]$TargetFramework = "",
    [switch]$Clean,
    [switch]$Test,
    [switch]$Pack,
    [switch]$Publish
)

$ErrorActionPreference = "Stop"

Write-Host "开始构建 AxolotlMCP..." -ForegroundColor Green

# 清理
if ($Clean) {
    Write-Host "清理项目..." -ForegroundColor Yellow
    dotnet clean --configuration $Configuration
    if ($LASTEXITCODE -ne 0) {
        Write-Error "清理失败"
        exit 1
    }
}

# 还原包
Write-Host "还原 NuGet 包..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "包还原失败"
    exit 1
}

# 构建
Write-Host "构建项目..." -ForegroundColor Yellow
$buildArgs = @("build", "--configuration", $Configuration, "--no-restore")
if ($TargetFramework) {
    $buildArgs += @("--framework", $TargetFramework)
}
dotnet @buildArgs
if ($LASTEXITCODE -ne 0) {
    Write-Error "构建失败"
    exit 1
}

# 测试
if ($Test) {
    Write-Host "运行测试..." -ForegroundColor Yellow
    dotnet test --configuration $Configuration --no-build --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        Write-Error "测试失败"
        exit 1
    }
}

# 打包
if ($Pack) {
    Write-Host "创建 NuGet 包..." -ForegroundColor Yellow
    dotnet pack --configuration $Configuration --no-build --output ./artifacts
    if ($LASTEXITCODE -ne 0) {
        Write-Error "打包失败"
        exit 1
    }
}

# 发布
if ($Publish) {
    Write-Host "发布项目..." -ForegroundColor Yellow
    $publishArgs = @("publish", "--configuration", $Configuration, "--no-build")
    if ($TargetFramework) {
        $publishArgs += @("--framework", $TargetFramework)
    }
    dotnet @publishArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Error "发布失败"
        exit 1
    }
}

Write-Host "构建完成!" -ForegroundColor Green
