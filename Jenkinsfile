pipeline {
    agent any

    stages {

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

        stage('Run Unit Tests') {
            steps {
                bat 'dotnet test .\\MyMusicLibrary.Tests\\MyMusicLibrary.Tests.csproj --configuration Debug --no-build'
            }
        }

        stage('Run Playwright Tests') {
            steps {
                bat 'dotnet test .\\MyMusicLibrary.PlaywrightTests\\MyMusicLibrary.PlaywrightTests.csproj --configuration Debug --no-build'
            }
        }
    }
}
