pipeline {
    agent any

    stages {

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Restore') {
            steps {
                bat 'dotnet restore'
            }
        }

        stage('Build') {
            steps {
                bat 'dotnet build --configuration Debug --no-restore'
            }
        }

        stage('Start Application') {
            steps {
                powershell '''
                    Write-Host "Starting MyMusicLibrary application..."

                    $process = Start-Process dotnet `
                        -ArgumentList "run --project .\\MyMusicLibrary\\MyMusicLibrary.csproj --no-build --urls=http://localhost:5000" `
                        -PassThru `
                        -RedirectStandardOutput "$env:WORKSPACE\\app-output.log" `
                        -RedirectStandardError "$env:WORKSPACE\\app-error.log"

                    $process.Id | Out-File "$env:WORKSPACE\\app.pid"

                    Write-Host "Application started with PID $($process.Id)"

                    Start-Sleep -Seconds 10

                    try {
                        Invoke-WebRequest -Uri "http://localhost:5049" -UseBasicParsing -TimeoutSec 10
                        Write-Host "Application is running."
                    }
                    catch {
                        Write-Host "Application did not respond."
                        Get-Content "$env:WORKSPACE\\app-error.log" -ErrorAction SilentlyContinue
                        exit 1
                    }
                '''
            }
        }

        stage('Run Unit Tests') {
            steps {
                bat 'dotnet test .\\MyMusicLibrary.Tests\\MyMusicLibrary.Tests.csproj --configuration Debug --no-build --logger "trx;LogFileName=unit-tests.trx"'
            }
        }

        stage('Run Playwright Tests') {
            steps {
                bat 'dotnet test .\\MyMusicLibrary.PlaywrightTests\\MyMusicLibrary.PlaywrightTests.csproj --configuration Debug --no-build --logger "trx;LogFileName=playwright-tests.trx"'
            }
        }
    }

    post {
        always {
            powershell '''
                Write-Host "Stopping application..."

                if (Test-Path "$env:WORKSPACE\\app.pid") {
                    $processId = Get-Content "$env:WORKSPACE\\app.pid"

                    Write-Host "Stopping application PID $processId"

                    Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
                }

                Write-Host "===== Application Output ====="

                if (Test-Path "$env:WORKSPACE\\app-output.log") {
                    Get-Content "$env:WORKSPACE\\app-output.log"
                }

                Write-Host "===== Application Errors ====="

                if (Test-Path "$env:WORKSPACE\\app-error.log") {
                    Get-Content "$env:WORKSPACE\\app-error.log"
                }
            '''

            archiveArtifacts artifacts: '**/*.trx, **/app-output.log, **/app-error.log',
                              allowEmptyArchive: true
        }
    }
}
