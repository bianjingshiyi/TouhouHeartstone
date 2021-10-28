#!groovy

@Library('lotlab-lib')
import org.lotlab.pipeline.QQBot
import org.lotlab.pipeline.Docker
import org.lotlab.pipeline.UnityBuilder
import org.lotlab.pipeline.TencentCOS
import org.lotlab.pipeline.BackblazeB2

pipeline {
    agent any
    options {
        timeout(time: 3, unit: 'HOURS')
        buildDiscarder logRotator(artifactDaysToKeepStr: '15', artifactNumToKeepStr: '15', daysToKeepStr: '90', numToKeepStr: '30')
    }
    stages {
        stage('checkout') {
            steps {
                script {
                    // 询问打包平台
                    try {
                        timeout(time:30, unit:'SECONDS') {
                            env.DEPLOY_TO = input(
                                message: '打哪个包？',
                                parameters: [
                                    [$class: 'TextParameterDefinition',
                                    defaultValue: 'release',
                                    description: '包配置', name: 'Debug 还是 Release']
                            ])
                        }
                    } catch (err) { // timeout reached or input Aborted
                        def user = err.getCauses()[0].getUser()
                        if ('SYSTEM' == user.toString()) { // SYSTEM means timeout
                            echo ('Input timeout expired, will build debug package.')
                            env.DEPLOY_TO = 'debug'
                        } else {
                            echo "Input aborted by: [${user}]"
                            error("Pipeline aborted by: [${user}]")
                        }
                    }

                    // 检出
                    def scmVars = checkout scm
                    env.GIT_PREVIOUS_SUCCESSFUL_COMMIT = scmVars.GIT_PREVIOUS_COMMIT
                    env.GIT_COMMIT = scmVars.GIT_COMMIT
                    echo "${GIT_COMMIT} to ${GIT_PREVIOUS_SUCCESSFUL_COMMIT}"

                    // 更新翻译
                    sh label: 'Pulling Latest Translation', script: '''cd ExternalData/i18n
                        git checkout master
                        git pull
                        cd ../../
                    '''
                    
                    // 设置构建版本
                    def gitTag = sh label: '', returnStdout: true, script: 'git describe --always'
                    env.BUILD_GIT_TAG = gitTag.trim()

                    // 清理过期构建
                    sh label: 'Clean Build Folder', script: 'rm -rf Build/*'
                }
            }
        }

        stage('Build Windows Debug') {
            steps {
                script {
                    UnityBuilder unitybuilder = new UnityBuilder(this, 'sample', 'unity_license', env.UNITY_CACHE_SERVER)
                    unitybuilder.buildPlatform('2019.4.31f1-windows-mono-0', 'win64-debug')
                }
            }
        }
        stage('Build Windows Release') {
            when {
                environment name: 'DEPLOY_TO', value: 'release'
            }
            steps {
                script {
                    UnityBuilder unitybuilder = new UnityBuilder(this, 'sample', 'unity_license', env.UNITY_CACHE_SERVER)
                    unitybuilder.buildPlatform('2019.4.31f1-windows-mono-0', 'win64-release;win32-release')
                }
            }
        }
        stage('Build Android Debug') {
            steps {
                script {
                    UnityBuilder unitybuilder = new UnityBuilder(this, 'sample', 'unity_license', env.UNITY_CACHE_SERVER)
                    unitybuilder.buildPlatform('2019.4.31f1-android-0', 'android-debug')
                }
            }
        }
        stage('Build Android Release') {
            when {
                environment name: 'DEPLOY_TO', value: 'release'
            }
            steps {
                script {
                    UnityBuilder unitybuilder = new UnityBuilder(this, 'sample', 'unity_license', env.UNITY_CACHE_SERVER)
                    unitybuilder.buildPlatform('2019.4.31f1-android-0', 'android-release')
                }
            }
        }
        stage('Build Linux') {
            when {
                environment name: 'DEPLOY_TO', value: 'release'
            }
            steps {
                script {
                    UnityBuilder unitybuilder = new UnityBuilder(this, 'sample', 'unity_license', env.UNITY_CACHE_SERVER)
                    unitybuilder.buildPlatform('2019.4.31f1-android-0', 'linux-debug;linux-release')
                }
            }
        }
        stage('Compress and Save') {
            steps {
                sh label: 'Compress package', script: '''#!/bin/bash
                    RELEASE_FOLDER=_artifact
                    PREFIX="醉梦传说_Rev_${BUILD_NUMBER}_"

                    cd Build
                    if [ -d $RELEASE_FOLDER ]; then
                        rm -r $RELEASE_FOLDER/*
                    else
                        mkdir -p $RELEASE_FOLDER
                    fi

                    for i in *
                    do
                    if [[ -d "$i" &&  $i != _* ]]; then
                        if [[ $i != *android* ]]; then
                            pushd .
                            cd $i
                            tar --gzip -cf ../$RELEASE_FOLDER/$PREFIX$i.tar.gz *
                            popd
                        else
                            cp $i/*.apk $RELEASE_FOLDER/$PREFIX$i.apk
                        fi
                    fi
                    done'''
                    archiveArtifacts artifacts: 'Build/_artifact/*', followSymlinks: false
            }
        }
        stage('Upload To Tencent COS') {
            when {
                anyOf {
                    environment name: 'DEPLOY_TO', value: 'release'
                    branch 'main'
                }
            }
            steps {
                script {
                    // Upload test package to TencentCloud
                    TencentCOS cos = new TencentCOS(this, 'sample', 'cos_igsk', 'igensoukyo-1251923006', 'ap-nanjing')
                    cos.upload("${env.WORKSPACE}/Build/_artifact", '*android_debug*', 'thg/nightly/')
                }
            }
        }
        stage('Upload To NetDisk') {
            steps {
                script {
                    BackblazeB2 b2 = new BackblazeB2(this, 'sample', 'backblaze-b2')
                    b2.syncUploadKeep("${env.WORKSPACE}/Build/_artifact", 'b2://igsk-ci/snapshots', '7')

                    def files = findFiles excludes: '', glob: 'Build/_artifact/**'
                    for (int i = 0; i < files.size(); i++) {
                        def filename = files[i].name
                        def url = "https://cdn.igsk.fun/snapshots/${filename} "
                        env.DOWNLOAD_LINK += url
                    }
                    echo "${env.DOWNLOAD_LINK}"
                }
            }
        }
    }

    post {
        success {
            script {
                if (env.BRANCH_NAME == 'main') {
                    def downloadURL = "https://igsk.cdn.bigkeer.cn/thg/nightly/醉梦传说_Rev_${BUILD_NUMBER}_android_debug.apk"
                    sshPublisher(publishers: [sshPublisherDesc(configName: 'IGensoukyo Test Server', transfers: [sshTransfer(cleanRemote: false, excludes: '', execCommand: "cd /home/jim/test-serv/account && ./new_update.sh ${BUILD_GIT_TAG} \"${downloadURL}\"", execTimeout: 120000, flatten: false, makeEmptyDirs: false, noDefaultExcludes: false, patternSeparator: '[, ]+', remoteDirectory: 'account', remoteDirectorySDF: false, removePrefix: '', sourceFiles: '')], usePromotionTimestamp: false, useWorkspaceInPromotion: false, verbose: true)])
                }
                mail bcc: '', body: "请访问 ${BUILD_URL} 查看下载地址和详细变更日志", cc: 'bianjingshiyi@hotmail.com', from: 'info@lotlab.org', replyTo: '', subject: "醉梦传说自动打包结果 - $BUILD_DISPLAY_NAME", to: 'jim@lotlab.org'
            }
        }
        failure {
            mail bcc: '', body: "打包失败了，请访问 ${BUILD_URL} 查阅相关的日志文件，修正错误后尝试重新打包", cc: 'bianjingshiyi@hotmail.com', from: 'info@lotlab.org', replyTo: '', subject: "醉梦传说打包失败通知 - $BUILD_DISPLAY_NAME", to: 'jim@lotlab.org'
        }
        always {
            script {
                def now = new Date()
                def buildtime = now.format('yyyy-MM-dd HH:mm', TimeZone.getTimeZone('Asia/Shanghai'))
                def logVar = [
                    [name: 'url', value: env.DOWNLOAD_LINK?.trim()],
                    [name: 'buildver', value: BUILD_DISPLAY_NAME],
                    [name: 'buildnum', value: BUILD_NUMBER],
                    [name: 'lastbuildnum', value: (BUILD_NUMBER.toInteger() - 1).toString()],
                    [name: 'nextbuildnum', value: (BUILD_NUMBER.toInteger() + 1).toString()],
                    [name: 'datetime', value: buildtime ],
                    [name: 'result', value: currentBuild.result],
                    [name: 'branch', value: env.BRANCH_NAME],
                    [name: 'projectUrl', value: env.BUILD_URL]
                ]

                String textLog = ''

                if (env.GIT_COMMIT) {
                    String textTemplate = readFile('ci/changelog.success.txt')
                    textLog = gitChangelog extendedVariables: logVar, from: [type: 'COMMIT', value: env.GIT_PREVIOUS_SUCCESSFUL_COMMIT], returnType: 'STRING', template: textTemplate, to: [type: 'COMMIT', value: env.GIT_COMMIT]
                } else {
                    String textTemplateFail = readFile('ci/changelog.fail.txt')
                    textLog = new groovy.text.SimpleTemplateEngine().createTemplate(textTemplateFail).make(logVar).toString()
                }

                if (textLog.length() > 1000) textLog = textLog.substring(0, 1000)

                def bot = new QQBot(this, env.QQ_BOT_HOST, 'token')
                bot.sendGroupMsg('1156816727', textLog)
                bot.sendGroupMsg('217522101', textLog)
            }
        }
    }
}
