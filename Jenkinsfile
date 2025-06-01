#!groovy

@Library('lotlab-lib')
import org.lotlab.pipeline.Wecom
import org.lotlab.pipeline.Webhook
import org.lotlab.pipeline.Docker
import org.lotlab.pipeline.Unity
import org.lotlab.pipeline.TencentCOS
import org.lotlab.pipeline.Minio
import org.lotlab.pipeline.Mail

import org.jenkinsci.plugins.workflow.steps.TimeoutStepExecution;
import org.jenkinsci.plugins.workflow.support.steps.input.Rejection;

pipeline {
    agent {
        node { label 'unity-2022.3-android' }
    }
    options {
        timeout(time: 2, unit: 'HOURS')
        buildDiscarder logRotator(artifactDaysToKeepStr: '15', artifactNumToKeepStr: '15', daysToKeepStr: '90', numToKeepStr: '30')
    }
    environment {
		AIFADIAN = credentials('aifadian-zmcs')
	}
    stages {
		stage('env-check') {
			steps {
                script {
                    // Default
                    env.IS_PR = 'false'
                    env.BUILD_RELEASE = 'false'
                    env.UPLOAD_FOLDER = 'snapshots'

                    if (env.BRANCH_NAME.startsWith('PR')) {
                        env.IS_PR = 'true'
                    }
                    if (env.BRANCH_NAME.startsWith('release')) {
                        env.BUILD_RELEASE = 'true'
                    }
                    if (env.BRANCH_NAME == 'snapshot') {
                        env.BUILD_RELEASE = 'true'
                        env.SNAPSHOT_BUILD = 'true'
                        env.UPLOAD_FOLDER = 'nightly'
                        env.GLOBAL_BUILD_FLAGS = ';SNAPSHOT_BUILD'
                    }
                }
			}
		}
        stage('checkout') {
            steps {
                script {
                    // 清理原有的工作空间
                    cleanWs deleteDirs: true, patterns: [[pattern: 'Library/**', type: 'INCLUDE'], [pattern: 'Temp/**', type: 'INCLUDE']]
                    // 清理过期构建
                    sh label: 'Clean Build Folder', script: 'rm -rf Build/*'

                    // 检出
                    def scmVars = checkout scm
                    echo "${scmVars.GIT_COMMIT} to ${scmVars.GIT_PREVIOUS_COMMIT}"

                    // 更新翻译
                    sh label: 'Pulling Latest Translation', script: '''cd ExternalData/i18n
                        git checkout master
                        git pull
                        cd ../../
                    '''
                    
                    // 设置构建版本
                    def gitTag = sh label: '', returnStdout: true, script: 'git describe --always'
                    env.BUILD_GIT_TAG = gitTag.trim()
                    
                    // 获取提交日志
                    env.COMMIT_MSG = sh label: '', returnStdout: true, script: 'git log HEAD -n 1 --format=%B'

                    // 生成文本 Git 日志
                    def logVar = [
                        [name: 'buildver', value: BUILD_DISPLAY_NAME],
                        [name: 'buildnum', value: BUILD_NUMBER],
                        [name: 'branch', value: env.BRANCH_NAME],
                        [name: 'projectUrl', value: env.BUILD_URL]
                    ]

                    try {
                        String textTemplate = readFile('ci/changelog.success.txt')
                        try {
                            env.GIT_CHANGELOG = gitChangelog(
                                extendedVariables: logVar, 
                                from: [type: 'COMMIT', value: scmVars.GIT_PREVIOUS_COMMIT], 
                                returnType: 'STRING', template: textTemplate, 
                                to: [type: 'COMMIT', value: scmVars.GIT_COMMIT]
                            )
                        } catch (Exception e) {
                            echo e.toString()
                            env.GIT_CHANGELOG = new groovy.text.SimpleTemplateEngine().createTemplate(textTemplate).make(logVar).toString()
                        }
                    } catch (Exception e) {
                        echo e.toString()
                        env.GIT_CHANGELOG = ""
                    }

                    if (env.GIT_CHANGELOG.length() > 10000)
                        env.GIT_CHANGELOG = env.GIT_CHANGELOG.substring(0, 10000)
                }
            }
        }
		stage('condition') {
			steps {
                script {
					if (env.COMMIT_MSG.contains('[ci release]')) {
						env.BUILD_RELEASE = 'true'
                        echo "Build release package trigger from commit"
                    }
					
					env.WINDOWS_PACKAGES = "win64-debug"
					env.LINUX_PACKAGES = ""
					env.ANDROID_PACKAGES = "android-debug"
					if (env.BUILD_RELEASE == 'true') {
						env.WINDOWS_PACKAGES += ';win64-release;win32-release'
						env.LINUX_PACKAGES += ';linux-debug;linux-release'
						env.ANDROID_PACKAGES += ';android-release'
					}
					if (env.COMMIT_MSG.contains('[ci win32]') && !env.WINDOWS_PACKAGES.contains('win32-debug')) {
						env.WINDOWS_PACKAGES += ';win32-debug'
					}
					if (env.COMMIT_MSG.contains('[ci linux]') && !env.LINUX_PACKAGES.contains('linux-debug')) {
						env.LINUX_PACKAGES += ';linux-debug'
					}
					if (env.LINUX_PACKAGES.length() > 0) {
						env.BUILD_LINUX = 'true'
					}
                }
			}
		}
        stage('Build Windows') {
            steps {
				container('unity') {
					script {
						env.ADDITIONAL_BUILD_FLAGS = env.GLOBAL_BUILD_FLAGS + ';STEAM_BUILD'
						def b = new Unity();
						b.setLicense('unity_license');
						b.startPlatform(env.WINDOWS_PACKAGES);
					}
                }
            }
        }
        stage('Build Android') {
            steps {
				container('unity-android') {
					script {
						def b = new Unity();
						b.setLicense('unity_license');
						b.withAndroidKey('zmcs_android') {
							b.startPlatform(env.ANDROID_PACKAGES);
						}
					}
                }
            }
        }
        stage('Build Linux') {
            when {
                environment name: 'BUILD_LINUX', value: 'true'
            }
            steps {
				container('unity-android') {
					script {
						env.ADDITIONAL_BUILD_FLAGS = env.GLOBAL_BUILD_FLAGS + ';STEAM_BUILD'
						def b = new Unity();
						b.setLicense('unity_license');
						b.startPlatform(env.LINUX_PACKAGES);
					}
				}
            }
        }
        stage('Compress') {
            steps {
				container('mc') {
					sh label: 'Compress package', script: '''#!/bin/sh
						RELEASE_FOLDER=_artifact
						PREFIX="醉梦传说_${BRANCH_NAME}_Rev_${BUILD_NUMBER}_"

						cd Build
						if [ -d $RELEASE_FOLDER ]; then
							rm -r $RELEASE_FOLDER/*
						else
							mkdir -p $RELEASE_FOLDER
						fi

						BUILD_DIR=$(pwd)
						
						for i in *
						do
						if [[ -d "$i" &&  $i != _* ]]; then
							if [[ $i != *android* ]]; then
								cd $BUILD_DIR/$i
								case $i in
									*debug* )
										echo "2825850" >> steam_appid.txt
										 ;;
								esac 
								zip -r ../$RELEASE_FOLDER/$PREFIX$i.zip *
							else
								cp $i/*.apk $RELEASE_FOLDER/$PREFIX$i.apk
							fi
						fi
						done'''
						// archiveArtifacts artifacts: 'Build/_artifact/*', followSymlinks: false
                }
            }
        }
        stage('Upload') {
            steps {
				container('mc') {
					script {
						Minio mc = new Minio(this, 'sample', 'b2', 'https://s3.us-west-002.backblazeb2.com', 'backblaze-b2')
						mc.login()
						
						def files = findFiles excludes: '', glob: 'Build/_artifact/**'
						env.DOWNLOAD_LINK = ''
						for (int i = 0; i < files.size(); i++) {
							def filename = files[i].name
							mc.cp("${env.WORKSPACE}/Build/_artifact/${filename}", "igsk-ci/${env.UPLOAD_FOLDER}/${filename}")
							def url = "https://cdn.igsk.fun/#/${env.UPLOAD_FOLDER}/${URLEncoder.encode(filename)}"
							env.DOWNLOAD_LINK += "${filename}: ${url} \n" 
						}
						echo "${env.DOWNLOAD_LINK}"
					}
				}
            }
        }
    }

    post {
        success {
            script {
                if (env.IS_PR == 'false') {
                    withCredentials([usernamePassword(credentialsId: 'WeworkMailToken', passwordVariable: 'token', usernameVariable: 'addr')]) {
                        def m = new Mail(this, env.addr, env.token)
                        try {
                            m.SendMail('ci-result@igsk.fun', "醉梦传说自动构建结果 - $BRANCH_NAME - $BUILD_DISPLAY_NAME".toString(), "项目地址： ${BUILD_URL} \n\n下载链接: \n${DOWNLOAD_LINK}\n\n提交日志: \n${GIT_CHANGELOG}".toString())
                        } catch (Exception e) {
                            echo e.toString()
                        }
                    }
                }
            }
        }
        failure {
            script {
                if (env.IS_PR == 'false') {
                    withCredentials([usernamePassword(credentialsId: 'WeworkMailToken', passwordVariable: 'token', usernameVariable: 'addr')]) {
                        def m = new Mail(this, env.addr, env.token)
                        try {
                            m.SendMail('ci-result@igsk.fun', "醉梦传说构建失败通知 - $BRANCH_NAME - $BUILD_DISPLAY_NAME".toString(), "打包失败了，请访问 ${BUILD_URL} 查阅相关的日志文件，修正错误后尝试重新打包\n\n提交日志: \n${GIT_CHANGELOG}".toString())
                        } catch (Exception e) {
                            echo e.toString()
                        }
                    }
                }
            }
        }
        always {
            script {
                String textLog = env.GIT_CHANGELOG
                if (textLog.length() > 1000) textLog = textLog.substring(0, 1000)

                def wecomContents = []
                def downloadMaps = [:]
                def description = '成品:<br>\n<ul>\n'
                
                def lines = []
                if (env.DOWNLOAD_LINK != null) {
                    lines = env.DOWNLOAD_LINK.split('\n')
                }
                for (int i = 0; i < lines.size(); i++) {
                    def cols = lines[i].split(': ')
                    if (cols.size() == 2) {
                        description += '<li><a href="' + cols[1] + '" target="_blank">' + cols[0] + '</a></li>\n'
                        downloadMaps[cols[0]] = cols[1]
                        if (i < 6) {
                            wecomContents += Wecom.contentItem('成品', cols[0], cols[1])
                        }
                    }
                }
                description += '</ul>'
                currentBuild.description = description
                    
                // 企业微信Webhook
                withCredentials([usernamePassword(credentialsId: 'jenkins_webhook', passwordVariable: 'token', usernameVariable: 'addr')]) {
                    def webhook = new Webhook(this, env.addr, env.token)
                    try {
                        webhook.sendResult(textLog, downloadMaps)
                    } catch (Exception e) {
                        echo e.toString();
                    }
                }
                // 企业微信机器人
                def linkList = []
                if (env.IS_PR == 'true') {
                    linkList += Wecom.jumpItem('合并请求', env.CHANGE_URL)
                }
                withCredentials([string(credentialsId: 'igsk_wecom_bot', variable: 'WECOM_TOKEN')]) {
                    def bot = new Wecom(this, env.WECOM_TOKEN)
                    try {
                        bot.sendResultCard(textLog, wecomContents, linkList)
                    } catch (Exception e) {
                        echo e.toString();
                    }
                }
            }
        }
    }
}
