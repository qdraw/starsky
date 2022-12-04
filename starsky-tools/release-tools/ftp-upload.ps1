# Parameters
param(
    [Parameter(Mandatory=$true)][String]$ftpUrl,
	[Parameter(Mandatory=$false)][String]$ftpBasePath,
	[Parameter(Mandatory=$true)][String]$ftpUser,
	[Parameter(Mandatory=$true)][String]$ftpPwd,
	[Parameter(Mandatory=$true)][String]$pathToDeploy
)

# Get files
$files = Get-ChildItem -Path $pathToDeploy -recurse  
# Get ftp object
$ftp_client = New-Object System.Net.WebClient
$ftp_client.Credentials = new-object System.Net.NetworkCredential($ftpUser, $ftpPwd) 
$ftp_address = $ftpUrl +"/" + $ftpBasePath
# Make uploads
foreach($file in $files)
{
    if ($file.Attributes -eq "Directory")
    {
        $directory = "";
        if ($file.FullName.Length -gt 0)
        {
			$directory = $file.FullName.Replace("\","/").Replace($pathToDeploy.Replace("\","/"),"")
        }

		try  
		{  
			"Creating dir $ftp_address/$directory"
			$makeDirectory = [System.Net.WebRequest]::Create( $ftp_address + $directory);
			$makeDirectory.Credentials = New-Object System.Net.NetworkCredential($ftpUser,$ftpPwd)
			$makeDirectory.Method = [System.Net.WebRequestMethods+FTP]::MakeDirectory;
			$makeDirectory.GetResponse();
		}  
		catch [System.Net.WebException]  
		{  
			$response = $_.Exception.Response;
			if ($response.StatusCode -eq [System.Net.FtpStatusCode]::ActionNotTakenFileUnavailable)
			{
				"Unable to create dir $ftp_address/$directory"  
			}
			else
			{
				Write-Error $_.Exception.Message
				Break
			}
		}  
    }
    else{
        $directory = "";
        $source = $file.DirectoryName + "\" + $file;
        if ($file.DirectoryName.Length -gt 0)
        {
			$directory = $file.DirectoryName.Replace("\","/").Replace($pathToDeploy.Replace("\","/"),"")
        }
        $directory += "/";
        $ftp_command = $ftp_address + $directory + $file
        $uri = New-Object System.Uri($ftp_command)
        "Uploading $directory$file..."
        $ftp_client.UploadFile($uri, $source)
    }
}