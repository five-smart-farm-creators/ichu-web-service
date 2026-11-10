#Requires -Version 7.0
<#
.SYNOPSIS
    Signs a Firebase user in to Google's Identity Toolkit and prepares the Development demo account.

.DESCRIPTION
    Checks that the Firebase user's email is verified, stores the user identifier and email as User environment
    variables so that the next Development start seeds the demo account, and copies the ID token to the clipboard.
    The password is sent only to Google, never to the ICHU API.

.PARAMETER Email
    Email of the Firebase user, created in Firebase Authentication.

.PARAMETER ApiBaseUrl
    Address of the ICHU API. It is used only in the instructions that the script prints.

.EXAMPLE
    ./scripts/firebase-dev-session.ps1 -Email manager@example.com
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$Email,

    [string]$ApiBaseUrl = 'http://localhost:5099'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$externalUidVariable = 'DevelopmentSeed__Manager__ExternalUid'
$emailVariable = 'DevelopmentSeed__Manager__Email'

# Firebase error codes that a developer can act on; other codes are printed as they are.
$firebaseErrors = @{
    EMAIL_NOT_FOUND             = 'No Firebase user has this email. Create the user in Firebase Authentication first.'
    INVALID_PASSWORD            = 'The password is incorrect.'
    INVALID_LOGIN_CREDENTIALS   = 'The email or password is incorrect.'
    INVALID_EMAIL               = 'The email address is not valid.'
    USER_DISABLED               = 'This Firebase user is disabled.'
    OPERATION_NOT_ALLOWED       = 'Email and password sign-in is disabled for this Firebase project.'
    TOO_MANY_ATTEMPTS_TRY_LATER = 'Too many attempts. Wait a few minutes, then run the script again.'
}

function Get-FirebaseApiKey {
    $apiKey = [Environment]::GetEnvironmentVariable('Firebase__ApiKey', 'User')
    if ([string]::IsNullOrWhiteSpace($apiKey)) {
        throw 'Set the User environment variable Firebase__ApiKey to the Firebase Web API key, then run the script again.'
    }

    return $apiKey
}

function ConvertTo-PlainText {
    param([Parameter(Mandatory)][securestring]$SecureValue)

    # The unmanaged copy is zeroed as soon as the managed string exists.
    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecureValue)
    try {
        return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
    }
}

function Get-FirebaseErrorMessage {
    param([Parameter(Mandatory)][System.Management.Automation.ErrorRecord]$ErrorRecord)

    $details = $ErrorRecord.ErrorDetails
    if ($null -eq $details -or [string]::IsNullOrWhiteSpace($details.Message)) {
        return "Google's Identity Toolkit could not be reached. Check the network connection and try again."
    }

    try {
        # Firebase appends a description to some codes after ' : ', which is not needed to recognize the error.
        $code = ($details.Message | ConvertFrom-Json).error.message -replace ' : .*$', ''
    }
    catch {
        return "Google's Identity Toolkit returned an unexpected response."
    }

    if ($firebaseErrors.ContainsKey($code)) {
        return $firebaseErrors[$code]
    }

    return "Firebase error: $code"
}

function Invoke-IdentityToolkit {
    param(
        [Parameter(Mandatory)][string]$Operation,
        [Parameter(Mandatory)][string]$ApiKey,
        [Parameter(Mandatory)][hashtable]$Body
    )

    $uri = 'https://identitytoolkit.googleapis.com/v1/accounts:{0}?key={1}' -f $Operation, [Uri]::EscapeDataString($ApiKey)
    try {
        return Invoke-RestMethod -Method Post -Uri $uri -ContentType 'application/json' -Body ($Body | ConvertTo-Json -Compress)
    }
    catch {
        # The request URI carries the API key, so only the translated message leaves this function.
        throw (Get-FirebaseErrorMessage -ErrorRecord $_)
    }
}

function Invoke-SignIn {
    param(
        [Parameter(Mandatory)][string]$Email,
        [Parameter(Mandatory)][securestring]$SecurePassword,
        [Parameter(Mandatory)][string]$ApiKey
    )

    $password = ConvertTo-PlainText -SecureValue $SecurePassword
    try {
        return Invoke-IdentityToolkit -Operation 'signInWithPassword' -ApiKey $ApiKey `
            -Body @{ email = $Email; password = $password; returnSecureToken = $true }
    }
    finally {
        $password = $null
    }
}

try {
    $apiKey = Get-FirebaseApiKey
    $securePassword = Read-Host -AsSecureString -Prompt "Password for $Email"
    if ($securePassword.Length -eq 0) {
        throw 'A password is required.'
    }

    $session = Invoke-SignIn -Email $Email -SecurePassword $securePassword -ApiKey $apiKey
    $lookup = Invoke-IdentityToolkit -Operation 'lookup' -ApiKey $apiKey -Body @{ idToken = $session.idToken }
    $user = $lookup.users | Select-Object -First 1
    if ($null -eq $user) {
        throw 'Firebase did not return the user for this session.'
    }

    if (-not $user.emailVerified) {
        Invoke-IdentityToolkit -Operation 'sendOobCode' -ApiKey $apiKey `
            -Body @{ requestType = 'VERIFY_EMAIL'; idToken = $session.idToken } | Out-Null
        Write-Host "The email is not verified. A verification email was sent to $($user.email). Open the link in that email, then run this script again."
        exit 1
    }

    # The API seeds the demo account only when both settings exist, so they are stored for its next start.
    [Environment]::SetEnvironmentVariable($externalUidVariable, $user.localId, 'User')
    [Environment]::SetEnvironmentVariable($emailVariable, $user.email, 'User')
    Set-Clipboard -Value $session.idToken

    Write-Host "Signed in as $($user.localId). The ID token was copied to the clipboard and expires in one hour."
    Write-Host "Restart the API in Development, open $ApiBaseUrl/swagger, click Authorize, paste the token without the Bearer prefix, click Authorize, then call GET api/v1/accounts/me/ranches."
    exit 0
}
catch {
    Write-Host $_.Exception.Message
    exit 1
}
