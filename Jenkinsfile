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
                bat 'dotnet build .\\MyMusicLibrary.Tests\\MyMusicLibrary.Tests.csproj --configuration Debug --no-restore'
            }
        }

        stage('Run MyMusicLibrary Tests') {
            steps {
                bat 'dotnet test .\\MyMusicLibrary.Tests\\MyMusicLibrary.Tests.csproj --configuration Debug --no-build'
            }
        }
    }
}
