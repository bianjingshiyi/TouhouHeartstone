#!groovy

@Library('lotlab-lib')
import org.lotlab.pipeline.QQBot
import org.lotlab.pipeline.Docker
import org.lotlab.pipeline.UnityBuilder
import org.lotlab.pipeline.TencentCOS

pipeline {
    agent any
    options {
        timeout(time: 3, unit: 'HOURS')
    }
    stages {
        stage('checkout') {
            options { retry(3) }
            steps {
                script {
                    def scmVars = checkout([$class: 'GitSCM', branches: [[name: '*/main']], doGenerateSubmoduleConfigurations: false, extensions: [[$class: 'GitLFSPull'], [$class: 'SubmoduleOption', disableSubmodules: false, parentCredentials: false, recursiveSubmodules: true, reference: '', trackingSubmodules: false]], submoduleCfg: [], userRemoteConfigs: [[credentialsId: 'lotlab_bot', url: 'https://code.lotlab.org/IGensokyo/TouhouHeartstone']]])
                    env.GIT_PREVIOUS_SUCCESSFUL_COMMIT = scmVars.GIT_PREVIOUS_COMMIT
                    env.GIT_COMMIT = scmVars.GIT_COMMIT
                    echo "${GIT_COMMIT} to ${GIT_PREVIOUS_SUCCESSFUL_COMMIT}"
                    def gitTag = sh label: '', returnStdout: true, script: 'git describe --always'
                    env.BUILD_GIT_TAG = gitTag.trim()
                }
            }
        }

        stage('Build Windows') {
            steps {
                script {
                    UnityBuilder unitybuilder = new UnityBuilder(this, 'sample', 'unity_license', env.UNITY_CACHE_SERVER)
                    unitybuilder.build('2019.4.8f1-windows', 'CustomBuilder.Editor.Builder.BuildWindows')
                }
            }
        }
        stage('Build Android') {
            steps {
                script {
                    UnityBuilder unitybuilder = new UnityBuilder(this, 'sample', 'unity_license', env.UNITY_CACHE_SERVER)
                    unitybuilder.build('2019.4.8f1-android', 'CustomBuilder.Editor.Builder.BuildAndroid')
                }
            }
        }
        stage('Build Linux') {
            steps {
                script {
                    UnityBuilder unitybuilder = new UnityBuilder(this, 'sample', 'unity_license', env.UNITY_CACHE_SERVER)
                    unitybuilder.build('2019.4.8f1-android', 'CustomBuilder.Editor.Builder.BuildLinux')
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
        stage('Upload') {
            steps {
                script {
                    // Upload test package to TencentCloud
                    TencentCOS cos = new TencentCOS(this, 'sample', 'cos_igsk', 'igensoukyo-1251923006', 'ap-nanjing')
                    cos.upload("${env.WORKSPACE}/Build/_artifact", '*android_debug*', 'thg/nightly/')

                    // Upload ALL package to CowTransfer (or other)
                    withCredentials([string(credentialsId: 'cow_cookie', variable: 'cow_cookie')]) {
                        script {
                            def result = sh label: '', returnStdout: true, script: "$JENKINS_HOME/transfer cow --cookie=\"${cow_cookie};\" -s $WORKSPACE/Build/artifact/ | grep 'Download Link' | awk '{print \$3}'"
                            env.DOWNLOAD_LINK = result
                            echo "${result}"
                        }
                    }
                }
            }
        }
    }

    post {
        success {
            script {
                def downloadURL = "https://igsk.cdn.bigkeer.cn/thg/nightly/醉梦传说_Rev_${BUILD_NUMBER}_android_debug.apk"
                sshPublisher(publishers: [sshPublisherDesc(configName: 'IGensoukyo Test Server', transfers: [sshTransfer(cleanRemote: false, excludes: '', execCommand: "cd /home/jim/test-serv/account && ./new_update.sh ${BUILD_GIT_TAG} \"${downloadURL}\"", execTimeout: 120000, flatten: false, makeEmptyDirs: false, noDefaultExcludes: false, patternSeparator: '[, ]+', remoteDirectory: 'account', remoteDirectorySDF: false, removePrefix: '', sourceFiles: '')], usePromotionTimestamp: false, useWorkspaceInPromotion: false, verbose: true)])
                mail bcc: '', body: '请访问 https://tools.lotlab.org/ci/ 查看下载地址和详细变更日志', cc: 'bianjingshiyi@hotmail.com', from: 'info@lotlab.org', replyTo: '', subject: "醉梦传说自动打包结果 - $BUILD_DISPLAY_NAME", to: 'jim@lotlab.org'
            }
        }
        failure {
            mail bcc: '', body: '打包失败了，请查阅相关的日志文件，修正错误后尝试重新打包', cc: 'bianjingshiyi@hotmail.com', from: 'info@lotlab.org', replyTo: '', subject: "醉梦传说打包失败通知 - $BUILD_DISPLAY_NAME", to: 'jim@lotlab.org'
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
                    [name: 'result', value: currentBuild.result]
                ]

                String log = ''
                String textLog = ''

                if (env.GIT_COMMIT) {
                    String htmlTemplate = readFile('ci/changelog.success.html')
                    String textTemplate = readFile('ci/changelog.success.txt')
                    log = gitChangelog extendedVariables: logVar, from: [type: 'COMMIT', value: env.GIT_PREVIOUS_SUCCESSFUL_COMMIT], returnType: 'STRING', template: htmlTemplate, to: [type: 'COMMIT', value: env.GIT_COMMIT]
                    textLog = gitChangelog extendedVariables: logVar, from: [type: 'COMMIT', value: env.GIT_PREVIOUS_SUCCESSFUL_COMMIT], returnType: 'STRING', template: textTemplate, to: [type: 'COMMIT', value: env.GIT_COMMIT]
                } else {
                    String htmlTemplateFail = readFile('ci/changelog.fail.html')
                    String textTemplateFail = readFile('ci/changelog.fail.txt')
                    log = new groovy.text.SimpleTemplateEngine().createTemplate(htmlTemplateFail).make(logVar).toString()
                    textLog = new groovy.text.SimpleTemplateEngine().createTemplate(textTemplateFail).make(logVar).toString()
                }

                writeFile encoding: 'UTF-8', file: 'Build/_html/index.html', text: log
                writeFile encoding: 'UTF-8', file: "Build/_html/${BUILD_NUMBER}.html", text: log
                sh script: "cat ${JENKINS_HOME}/jobs/${JOB_NAME}/builds/${BUILD_NUMBER}/log >> Build/_html/${BUILD_NUMBER}.log"
                sshPublisher(
                    publishers: [
                        sshPublisherDesc(
                            configName: 'Lotlab Website',
                            transfers: [
                                sshTransfer(
                                    cleanRemote: false,
                                    excludes: '',
                                    execCommand: '',
                                    execTimeout: 120000,
                                    flatten: false,
                                    makeEmptyDirs: false,
                                    noDefaultExcludes: false,
                                    patternSeparator: '[, ]+',
                                    remoteDirectory: 'tools/ci',
                                    remoteDirectorySDF: false,
                                    removePrefix: 'Build/_html/',
                                    sourceFiles: 'Build/_html/*.html,Build/_html/*.log'
                                )
                            ],
                            usePromotionTimestamp: false,
                            useWorkspaceInPromotion: false,
                            verbose: false
                        )
                    ]
                )

                if (textLog.length() > 1000) textLog = textLog.substring(0, 1000)

                def bot = new QQBot(this, env.QQ_BOT_HOST, 'token')
                bot.sendGroupMsg('1156816727', textLog)
                bot.sendGroupMsg('217522101', textLog)
            }
        }
    }
}
