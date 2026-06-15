param(
    [ValidateSet("check", "migrate", "run", "all")]
    [string]$Mode = "all",
    [switch]$SkipBuild,
    [switch]$SkipInfraChecks,
    [switch]$Headless
)

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$SolutionPath = Join-Path $RepoRoot "MultiShop.sln"

function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Assert-LastExitCode {
    param([string]$StepName)
    if ($LASTEXITCODE -ne 0) {
        throw "$StepName basarisiz oldu (exit code: $LASTEXITCODE)."
    }
}

function Stop-MultiShopDotnetProcesses {
    $processNames = @(
        "MultiShop.WebUI",
        "MultiShop.OcelotGateway",
        "MultiShop.IdentityServer",
        "MultiShop.Catalog",
        "MultiShop.Discount",
        "MultiShop.Order.WebApi",
        "MultiShop.Cargo.WebApi",
        "MultiShop.Basket",
        "MultiShop.Comment",
        "MultiShop.Payment",
        "MultiShop.Images",
        "MultiShop.Message",
        "MultiShop.Favorite"
    )

    foreach ($name in $processNames) {
        Get-Process -Name $name -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
    }
}

function Assert-Command {
    param([string]$Name)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "'$Name' komutu bulunamadi. Lutfen kurup tekrar dene."
    }
}

function Test-Port {
    param(
        [string]$ComputerName,
        [int]$Port,
        [string]$Label
    )

    try {
        $result = Test-NetConnection -ComputerName $ComputerName -Port $Port -WarningAction SilentlyContinue
        if ($result.TcpTestSucceeded) {
            Write-Host "[OK] $Label -> $ComputerName`:$Port"
        }
        else {
            Write-Warning "[BAGLANTI YOK] $Label -> $ComputerName`:$Port"
        }
    }
    catch {
        Write-Warning "[KONTROL HATASI] $Label -> $ComputerName`:$Port"
    }
}

function Test-SqlInstance {
    param(
        [string]$Instance = ".\SQLEXPRESS"
    )

    if (-not (Get-Command "sqlcmd" -ErrorAction SilentlyContinue)) {
        Write-Warning "[KONTROL ATLANDI] sqlcmd bulunamadi, SQL instance test edilemedi."
        return
    }

    sqlcmd -S $Instance -Q "SELECT 1" 1>$null 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] SQL Server baglantisi -> $Instance"
    }
    else {
        Write-Warning "[BAGLANTI YOK] SQL Server baglantisi -> $Instance"
    }
}

function Test-HttpUrl {
    param(
        [string]$Url,
        [string]$Label
    )

    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 8
        Write-Host "[OK] $Label -> $($response.StatusCode)"
    }
    catch {
        if ($_.Exception.Response) {
            Write-Warning "[HTTP $([int]$_.Exception.Response.StatusCode)] $Label -> $Url"
        }
        else {
            Write-Warning "[ERISILEMIYOR] $Label -> $Url"
        }
    }
}

function New-ManagerAccessToken {
    param(
        [string]$IdentityUrl = "http://localhost:5001"
    )

    $suffix = Get-Date -Format "yyyyMMddHHmmss"
    $username = "health.$suffix"
    $password = "Aa123456!"

    $registerBody = @{
        name            = "Health"
        surname         = "Check"
        username        = $username
        email           = "$username@local.test"
        password        = $password
        confirmPassword = $password
    } | ConvertTo-Json

    Invoke-RestMethod -Method Post -Uri "$IdentityUrl/api/registers" -ContentType "application/json" -Body $registerBody | Out-Null

    $scope = "CatalogReadPermission CatalogFullPermission BasketFullPermission OcelotFullPermission CommentFullPermission PaymentFullPermission ImageFullPermission DiscountFullPermission OrderFullPermisson MessageFullPermission CargoFullPermission openid email profile"
    $tokenResponse = Invoke-RestMethod -Method Post -Uri "$IdentityUrl/connect/token" -ContentType "application/x-www-form-urlencoded" -Body "client_id=MultiShopManagerId&client_secret=multishopsecret&grant_type=password&username=$username&password=$password&scope=$scope"

    return $tokenResponse.access_token
}

function Run-SmokeChecks {
    Write-Step "Kritik URL smoke test"
    Test-HttpUrl -Url "http://localhost:5178/" -Label "WebUI"
    Test-HttpUrl -Url "http://localhost:5001/.well-known/openid-configuration" -Label "Identity metadata"
    Test-HttpUrl -Url "http://localhost:7070/swagger/index.html" -Label "Catalog Swagger"
    Test-HttpUrl -Url "http://localhost:7071/swagger/index.html" -Label "Discount Swagger"
    Test-HttpUrl -Url "http://localhost:7072/swagger/index.html" -Label "Order Swagger"
    Test-HttpUrl -Url "http://localhost:7073/swagger/index.html" -Label "Cargo Swagger"
    Test-HttpUrl -Url "http://localhost:7074/swagger/index.html" -Label "Basket Swagger"
    Test-HttpUrl -Url "http://localhost:7075/swagger/index.html" -Label "Comment Swagger"
    Test-HttpUrl -Url "http://localhost:7078/swagger/index.html" -Label "Message Swagger"

    Write-Step "Ocelot route bazli smoke test"
    try {
        $token = New-ManagerAccessToken
        if (-not $token) {
            throw "Token alinamadi"
        }

        $headers = @{ Authorization = "Bearer $token" }
        $gatewayTests = @(
            @{ Label = "Gateway Catalog";  Url = "http://localhost:5000/services/catalog/categories" },
            @{ Label = "Gateway Discount"; Url = "http://localhost:5000/services/discount/discounts/GetDiscountCouponCount" },
            @{ Label = "Gateway Order";    Url = "http://localhost:5000/services/order/addresses" },
            @{ Label = "Gateway Cargo";    Url = "http://localhost:5000/services/cargo/cargocompanies" },
            @{ Label = "Gateway Basket";   Url = "http://localhost:5000/services/basket/baskets" },
            @{ Label = "Gateway Message";  Url = "http://localhost:5000/services/message/usermessage/GetTotalMessageCount" },
            @{ Label = "Gateway Comment";  Url = "http://localhost:5000/services/comment/comments/GetTotalCommentCount" },
            @{ Label = "Gateway Payment";  Url = "http://localhost:5000/services/payment/payments" },
            @{ Label = "Gateway Images";   Url = "http://localhost:5000/services/images/weatherforecast" }
        )

        foreach ($test in $gatewayTests) {
            try {
                $response = Invoke-WebRequest -Uri $test.Url -Headers $headers -UseBasicParsing -TimeoutSec 10
                Write-Host "[OK] $($test.Label) -> $($response.StatusCode)"
            }
            catch {
                if ($_.Exception.Response) {
                    Write-Warning "[HTTP $([int]$_.Exception.Response.StatusCode)] $($test.Label) -> $($test.Url)"
                }
                else {
                    Write-Warning "[ERISILEMIYOR] $($test.Label) -> $($test.Url)"
                }
            }
        }
    }
    catch {
        Write-Warning "Ocelot route smoke test atlandi: $($_.Exception.Message)"
    }
}

function Invoke-EfMigration {
    param(
        [string]$Project,
        [string]$StartupProject
    )

    $projectPath = Join-Path $RepoRoot $Project
    $startupPath = Join-Path $RepoRoot $StartupProject

    Write-Host "Migration -> $Project"
    dotnet ef database update --project "$projectPath" --startup-project "$startupPath"
}

function Start-ServiceWindow {
    param(
        [string]$Name,
        [string]$Project,
        [string]$Urls
    )

    $projectPath = Join-Path $RepoRoot $Project
    $escapedRepo = $RepoRoot.Replace("'", "''")
    $escapedProject = $projectPath.Replace("'", "''")
    $escapedUrls = $Urls.Replace("'", "''")
    $title = "MultiShop - $Name"

    $command = @"
`$Host.UI.RawUI.WindowTitle = '$title'
Set-Location '$escapedRepo'
dotnet run --no-build --project '$escapedProject' --urls '$escapedUrls'
"@

    Start-Process powershell -ArgumentList @("-NoExit", "-Command", $command) | Out-Null
    Write-Host "[BASLATILDI] $Name -> $Urls"
}

function Start-ServiceHeadless {
    param(
        [string]$Name,
        [string]$Project,
        [string]$Urls
    )

    $projectPath = Join-Path $RepoRoot $Project
    $logsDir = Join-Path $RepoRoot "logs"
    if (-not (Test-Path $logsDir)) {
        New-Item -Path $logsDir -ItemType Directory | Out-Null
    }

    $safeName = $Name.Replace(" ", "_")
    $outLog = Join-Path $logsDir "$safeName.out.log"
    $errLog = Join-Path $logsDir "$safeName.err.log"

    $argLine = "run --no-build --project `"$projectPath`" --urls `"$Urls`""
    Start-Process -FilePath "dotnet" -ArgumentList $argLine -WindowStyle Hidden -RedirectStandardOutput $outLog -RedirectStandardError $errLog | Out-Null
    Write-Host "[BASLATILDI-HEADLESS] $Name -> $Urls (log: logs\\$safeName.*.log)"
}

function Run-Checks {
    Write-Step "On gereksinim kontrolleri"
    Assert-Command "dotnet"
    Assert-Command "powershell"

    $sdkList = dotnet --list-sdks
    $sdkVersions = @()
    foreach ($line in $sdkList) {
        if ($line -match "^(\d+)\.(\d+)\.") {
            $sdkVersions += [version]("$($matches[1]).$($matches[2]).0")
        }
    }

    $hasSdk6 = $sdkList -match "^6\."
    $hasSdkGte6 = $sdkVersions | Where-Object { $_ -ge [version]"6.0.0" }

    if ($hasSdk6) {
        Write-Host "[OK] .NET 6 SDK bulundu."
    }
    elseif ($hasSdkGte6) {
        Write-Host "[INFO] .NET 6 SDK yok, fakat daha yeni SDK mevcut. Build genelde calisir."
    }
    else {
        Write-Warning ".NET SDK 6+ bulunamadi. Proje derlenemeyebilir."
    }

    if (-not $SkipInfraChecks) {
        Write-Step "Altyapi port kontrolleri"
        Test-SqlInstance -Instance ".\SQLEXPRESS"
        Test-Port -ComputerName "localhost" -Port 27017 -Label "MongoDB (Catalog)"
        Test-Port -ComputerName "localhost" -Port 5432 -Label "PostgreSQL (Message)"
        Test-Port -ComputerName "localhost" -Port 6379 -Label "Redis (Basket)"
    }

    if (-not $SkipBuild) {
        Write-Step "Eski MultiShop proseslerini durdurma (dosya kilidi onleme)"
        Stop-MultiShopDotnetProcesses

        Write-Step "Restore + Build"
        dotnet restore "$SolutionPath"
        Assert-LastExitCode "dotnet restore"
        dotnet build "$SolutionPath" -c Debug
        Assert-LastExitCode "dotnet build"
    }
    else {
        Write-Host "Build adimi atlandi (--SkipBuild)."
    }

    if (-not $SkipInfraChecks) {
        Run-SmokeChecks
    }
    else {
        Write-Host "Smoke test adimi atlandi (--SkipInfraChecks)."
    }
}

function Run-Migrations {
    Write-Step "EF migration adimlari"
    Assert-Command "dotnet"
    dotnet tool restore

    Invoke-EfMigration -Project "IdentityServer\MultiShop.IdentityServer\MultiShop.IdentityServer.csproj" -StartupProject "IdentityServer\MultiShop.IdentityServer\MultiShop.IdentityServer.csproj"
    Invoke-EfMigration -Project "Services\Order\Infrastructure\MultiShop.Order.Persistence\MultiShop.Order.Persistence.csproj" -StartupProject "Services\Order\Presentation\MultiShop.Order.WebApi\MultiShop.Order.WebApi.csproj"
    Invoke-EfMigration -Project "Services\Cargo\MultiShop.Cargo.DataAccessLayer\MultiShop.Cargo.DataAccessLayer.csproj" -StartupProject "Services\Cargo\MultiShop.Cargo.WebApi\MultiShop.Cargo.WebApi.csproj"
    Invoke-EfMigration -Project "Services\Comment\MultiShop.Comment\MultiShop.Comment.csproj" -StartupProject "Services\Comment\MultiShop.Comment\MultiShop.Comment.csproj"
    Invoke-EfMigration -Project "Services\Discount\MultiShop.Discount\MultiShop.Discount.csproj" -StartupProject "Services\Discount\MultiShop.Discount\MultiShop.Discount.csproj"
    Invoke-EfMigration -Project "Services\Message\MultiShop.Message\MultiShop.Message.csproj" -StartupProject "Services\Message\MultiShop.Message\MultiShop.Message.csproj"
}

function Run-Services {
    if ($Headless) {
        Write-Step "Mikroservisleri arka planda baslatma (headless)"
    }
    else {
        Write-Step "Mikroservisleri ayri pencerelerde baslatma"
    }

    # `dotnet run` (no --no-build) recompiles each project; WebUI is last -> port 5178 stays dead for minutes
    # and the browser may open before Kestrel listens. Mode=run: one sln build, then all processes use --no-build.
    # Mode=all: Run-Checks already built the solution.
    if (-not $SkipBuild) {
        if ($Mode -eq "run") {
            Assert-Command "dotnet"
            Write-Step "Eski MultiShop proseslerini durdurma (dosya kilidi onleme)"
            Stop-MultiShopDotnetProcesses

            Write-Step "Once toplu build (hizli baslatma; 5178 hemen dinlemeye yakin)"
            dotnet restore "$SolutionPath"
            Assert-LastExitCode "dotnet restore"
            dotnet build "$SolutionPath" -c Debug
            Assert-LastExitCode "dotnet build"
        }
    }
    else {
        Write-Host "[INFO] -SkipBuild: onceden `dotnet build` gerekir; aksi halde `dotnet run --no-build` basarisiz olur."
    }

    if ($Headless) {
        Start-ServiceHeadless -Name "Identity" -Project "IdentityServer\MultiShop.IdentityServer\MultiShop.IdentityServer.csproj" -Urls "http://localhost:5001"
    }
    else {
        Start-ServiceWindow -Name "Identity" -Project "IdentityServer\MultiShop.IdentityServer\MultiShop.IdentityServer.csproj" -Urls "http://localhost:5001"
    }

    Start-RemainingServices
}

function Start-RemainingServices {
    if ($Headless) {
        Start-ServiceHeadless -Name "Catalog" -Project "Services\Catalog\MultiShop.Catalog\MultiShop.Catalog.csproj" -Urls "http://localhost:7070"
        Start-ServiceHeadless -Name "Discount" -Project "Services\Discount\MultiShop.Discount\MultiShop.Discount.csproj" -Urls "http://localhost:7071"
        Start-ServiceHeadless -Name "Order" -Project "Services\Order\Presentation\MultiShop.Order.WebApi\MultiShop.Order.WebApi.csproj" -Urls "http://localhost:7072"
        Start-ServiceHeadless -Name "Cargo" -Project "Services\Cargo\MultiShop.Cargo.WebApi\MultiShop.Cargo.WebApi.csproj" -Urls "http://localhost:7073"
        Start-ServiceHeadless -Name "Basket" -Project "Services\Basket\MultiShop.Basket\MultiShop.Basket.csproj" -Urls "http://localhost:7074"
        Start-ServiceHeadless -Name "Comment" -Project "Services\Comment\MultiShop.Comment\MultiShop.Comment.csproj" -Urls "http://localhost:7075"
        Start-ServiceHeadless -Name "Payment" -Project "Services\Payment\MultiShop.Payment\MultiShop.Payment.csproj" -Urls "http://localhost:7076"
        Start-ServiceHeadless -Name "Images" -Project "Services\Images\MultiShop.Images\MultiShop.Images.csproj" -Urls "http://localhost:7077"
        Start-ServiceHeadless -Name "Message" -Project "Services\Message\MultiShop.Message\MultiShop.Message.csproj" -Urls "http://localhost:7078"
        Start-ServiceHeadless -Name "Favorite" -Project "Services\Favorite\MultiShop.Favorite\MultiShop.Favorite.csproj" -Urls "http://localhost:7079"
        Start-ServiceHeadless -Name "Gateway" -Project "ApiGateway\MultiShop.OcelotGateway\MultiShop.OcelotGateway.csproj" -Urls "http://localhost:5000"
        Start-ServiceHeadless -Name "WebUI" -Project "Frontends\MultiShop.WebUI\MultiShop.WebUI.csproj" -Urls "http://localhost:5178"
    }
    else {
        Start-ServiceWindow -Name "Catalog" -Project "Services\Catalog\MultiShop.Catalog\MultiShop.Catalog.csproj" -Urls "http://localhost:7070"
        Start-ServiceWindow -Name "Discount" -Project "Services\Discount\MultiShop.Discount\MultiShop.Discount.csproj" -Urls "http://localhost:7071"
        Start-ServiceWindow -Name "Order" -Project "Services\Order\Presentation\MultiShop.Order.WebApi\MultiShop.Order.WebApi.csproj" -Urls "http://localhost:7072"
        Start-ServiceWindow -Name "Cargo" -Project "Services\Cargo\MultiShop.Cargo.WebApi\MultiShop.Cargo.WebApi.csproj" -Urls "http://localhost:7073"
        Start-ServiceWindow -Name "Basket" -Project "Services\Basket\MultiShop.Basket\MultiShop.Basket.csproj" -Urls "http://localhost:7074"
        Start-ServiceWindow -Name "Comment" -Project "Services\Comment\MultiShop.Comment\MultiShop.Comment.csproj" -Urls "http://localhost:7075"
        Start-ServiceWindow -Name "Payment" -Project "Services\Payment\MultiShop.Payment\MultiShop.Payment.csproj" -Urls "http://localhost:7076"
        Start-ServiceWindow -Name "Images" -Project "Services\Images\MultiShop.Images\MultiShop.Images.csproj" -Urls "http://localhost:7077"
        Start-ServiceWindow -Name "Message" -Project "Services\Message\MultiShop.Message\MultiShop.Message.csproj" -Urls "http://localhost:7078"
        Start-ServiceWindow -Name "Favorite" -Project "Services\Favorite\MultiShop.Favorite\MultiShop.Favorite.csproj" -Urls "http://localhost:7079"
        Start-ServiceWindow -Name "Gateway" -Project "ApiGateway\MultiShop.OcelotGateway\MultiShop.OcelotGateway.csproj" -Urls "http://localhost:5000"
        Start-ServiceWindow -Name "WebUI" -Project "Frontends\MultiShop.WebUI\MultiShop.WebUI.csproj" -Urls "http://localhost:5178"
    }
}

switch ($Mode) {
    "check" {
        Run-Checks
    }
    "migrate" {
        Run-Migrations
    }
    "run" {
        Run-Services
    }
    "all" {
        Run-Checks
        Run-Migrations
        Run-Services
    }
}

Write-Step "Tamamlandi"
Write-Host "Mode: $Mode"
Write-Host "Not: altyapi servislerini (SQL/Mongo/Postgres/Redis) once ayaga kaldirdigindan emin ol."
if ($Mode -in @("run", "all")) {
    Write-Host "WebUI (5178) Kestrel baslangici ~5-30 sn surebilir; aninda hata gorsen F5 / logs\WebUI.out.log'da 'Now listening' bekle."
}
