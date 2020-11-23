from ...shared.constants import TEST_PROJECTS_DIR, PATH_UNITY_REVISION, PATH_TEST_RESULTS, PATH_PLAYERS, UTR_INSTALL_URL, UNITY_DOWNLOADER_CLI_URL, get_unity_downloader_cli_cmd, get_timeout
from ruamel.yaml.scalarstring import PreservedScalarString as pss
from ...shared.utr_utils import  extract_flags


def _cmd_base(project_folder, platform, editor):
    return []

def cmd_editmode(project_folder, platform, api, test_platform, editor, build_config, color_space):
    utr_args = extract_flags(test_platform["utr_flags"], platform["name"], api["name"], build_config, color_space, project_folder)

    base = [
        f'pip install unity-downloader-cli --index-url {UNITY_DOWNLOADER_CLI_URL} --upgrade',
        f'unity-downloader-cli { get_unity_downloader_cli_cmd(editor, platform["os"]) } {"".join([f"-c {c} " for c in platform["components"]])}  --wait --published-only',
        f'curl -s {UTR_INSTALL_URL} --output utr',
        f'chmod +x ./utr',
        pss(f'''
         export GIT_REVISIONDATE=`git rev-parse HEAD | git show -s --format=%cI`
        ./utr {" ".join(utr_args)}''')
     ]

    extra_cmds = extra_perf_cmd(project_folder)
    unity_config = install_unity_config(project_folder)
    extra_cmds = extra_cmds + unity_config
    if project_folder.lower() == "BoatAttack".lower():
        base = extra_cmds + base
    return base

def cmd_playmode(project_folder, platform, api, test_platform, editor, build_config, color_space):
    utr_args = extract_flags(test_platform["utr_flags"], platform["name"], api["name"], build_config, color_space, project_folder)

    base = [
        f'pip install unity-downloader-cli --index-url {UNITY_DOWNLOADER_CLI_URL} --upgrade',
        f'unity-downloader-cli { get_unity_downloader_cli_cmd(editor, platform["os"]) } {"".join([f"-c {c} " for c in platform["components"]])}  --wait --published-only',
        f'curl -s {UTR_INSTALL_URL} --output utr',
        f'chmod +x ./utr',
        pss(f'''
         export GIT_REVISIONDATE=`git rev-parse HEAD | git show -s --format=%cI`
        ./utr {" ".join(utr_args)}''')
     ]
    extra_cmds = extra_perf_cmd(project_folder)
    unity_config = install_unity_config(project_folder)
    extra_cmds = extra_cmds + unity_config
    if project_folder.lower() == "BoatAttack".lower():
        base = extra_cmds + base
    return base

def cmd_standalone(project_folder, platform, api, test_platform, editor, build_config, color_space):
    utr_args = extract_flags(test_platform["utr_flags"], platform["name"], api["name"], build_config, color_space, project_folder)
    scripting_backend = build_config["scripting_backend"]
    api_level = build_config["api_level"]

    quality_levels = []

    for utr_arg in utr_args:
        if ';' in utr_arg:
            test_filters = utr_arg.split('=')
            quality_level = test_filters[1][1:-1]
            quality_levels = quality_level.split(';')
            utr_args.remove(utr_arg)

    utr_commands = []

    if len(quality_levels) > 0:
        for q in quality_levels:
            str_in_list = any('testfilter' in string for string in utr_args)
            if str_in_list == False:
                testfilter = f'--testfilter={q}'
                utr_args.append(testfilter)
                tail = ''
                for arg in utr_args:
                    if 'players' in arg:
                        #stripped = arg.split('players', 1)[0]
                        head, sep, tail = arg.partition('players')
                utr_args = [arg.replace(tail, q) for arg in utr_args]
                utr_command = f'./utr {" ".join(utr_args)}'
                utr_commands.append(utr_command)
            else:
                utr_args.pop()
                testfilter = f'--testfilter={q}'
                utr_args.append(testfilter)
                tail = ''
                for arg in utr_args:
                    if 'players' in arg:
                        #stripped = arg.split('players', 1)[0]
                        head, sep, tail = arg.partition('players')
                utr_args = [arg.replace(tail, q) for arg in utr_args]
                utr_command = f'        ./utr {" ".join(utr_args)}'
                utr_commands.append(utr_command)

    utr_args = [arg.replace('<TEST_FILTER>', '') for arg in utr_args]

    base = [
        f'curl -s {UTR_INSTALL_URL} --output utr',
        f'chmod +x ./utr'
     ]

    
    git_utr_string = ''
    utr_string = ''
    if len(utr_commands) > 0:
        for cmd in utr_commands:
            git_utr_string =        pss(f'''
         export GIT_REVISIONDATE=`git rev-parse HEAD | git show -s --format=%cI`
        {cmd}''')
            base.append(git_utr_string)
    else:
        utr_string = utr_string + f'./utr {" ".join(utr_args)}'
        git_utr_string =        pss(f'''
         export GIT_REVISIONDATE=`git rev-parse HEAD | git show -s --format=%cI`
        {utr_string}''')
        base.append(git_utr_string)
    
     
    return base

        
def cmd_standalone_build(project_folder, platform, api, test_platform, editor, build_config, color_space):
    utr_args = extract_flags(test_platform["utr_flags_build"], platform["name"], api["name"], build_config, color_space, project_folder)

    quality_levels = []

    for utr_arg in utr_args:
        if ';' in utr_arg:
            test_filters = utr_arg.split('=')
            quality_level = test_filters[1][1:-1]
            quality_levels = quality_level.split(';')
            utr_args.remove(utr_arg)

    utr_commands = []

    if len(quality_levels) > 0:
        for q in quality_levels:
            str_in_list = any('testfilter' in string for string in utr_args)
            if str_in_list == False:
                testfilter = f'--testfilter={q}'
                utr_args.append(testfilter)
                tail = ''
                for arg in utr_args:
                    if 'players' in arg:
                        #stripped = arg.split('players', 1)[0]
                        head, sep, tail = arg.partition('players')
                utr_args = [arg.replace(tail, q) for arg in utr_args] 
                utr_command = f'./utr {" ".join(utr_args)}'
                utr_commands.append(utr_command)
            else:
                utr_args.pop()
                testfilter = f'--testfilter={q}'
                utr_args.append(testfilter)
                tail = ''
                for arg in utr_args:
                    if 'players' in arg:
                        #stripped = arg.split('players', 1)[0]
                        head, sep, tail = arg.partition('players')
                utr_args = [arg.replace(tail, q) for arg in utr_args]
                utr_command = f'        ./utr {" ".join(utr_args)}'
                utr_commands.append(utr_command)

    utr_args = [arg.replace('<TEST_FILTER>', '') for arg in utr_args]

    base = [
        f'pip install unity-downloader-cli --index-url {UNITY_DOWNLOADER_CLI_URL} --upgrade',
        f'unity-downloader-cli { get_unity_downloader_cli_cmd(editor, platform["os"]) } {"".join([f"-c {c} " for c in platform["components"]])}  --wait --published-only',
        f'curl -s {UTR_INSTALL_URL} --output utr',
        f'chmod +x ./utr'
     ]
    
    git_utr_string = ''
    utr_string = ''
    if len(utr_commands) > 0:
        for cmd in utr_commands:
            git_utr_string =        pss(f'''
         export GIT_REVISIONDATE=`git rev-parse HEAD | git show -s --format=%cI`
        {cmd}''')
            base.append(git_utr_string)
    else:
        utr_string = utr_string + f'./utr {" ".join(utr_args)}'
        git_utr_string =        pss(f'''
         export GIT_REVISIONDATE=`git rev-parse HEAD | git show -s --format=%cI`
        {utr_string}''')
        base.append(git_utr_string)
    

    extra_cmds = extra_perf_cmd(project_folder)
    unity_config = install_unity_config(project_folder)
    extra_cmds = extra_cmds + unity_config
    if project_folder.lower() == "BoatAttack".lower():
        base = extra_cmds + base
    return base

def extra_perf_cmd(project_folder):   
    perf_list = [
        f'git clone https://github.com/Unity-Technologies/BoatAttack.git -b $BOAT_ATTACK_BRANCH TestProjects/{project_folder}',
        f'cd TestProjects/{project_folder} && git checkout $BOAT_ATTACK_REVISION'
        ]
    return perf_list

def install_unity_config(project_folder):
    cmds = [
        f'brew tap --force-auto-update unity/unity git@github.cds.internal.unity3d.com:unity/homebrew-unity.git',
        f'brew install unity-config',

        f'cd {TEST_PROJECTS_DIR}/{project_folder} && unity-config project add dependency "com.unity.render-pipelines.core@file:../../../com.unity.render-pipelines.core" --project-path .',
        f'cd {TEST_PROJECTS_DIR}/{project_folder} && unity-config project add dependency "com.unity.render-pipelines.universal@file:../../../com.unity.render-pipelines.universal" --project-path .',
        f'cd {TEST_PROJECTS_DIR}/{project_folder} && unity-config project add dependency "com.unity.shadergraph@file:../../../com.unity.shadergraph" --project-path .',


		#f'cd {TEST_PROJECTS_DIR}/{project_folder} && unity-config project remove dependency com.unity.render-pipelines.universal',
        f'cd {TEST_PROJECTS_DIR}/{project_folder} && unity-config project add dependency com.unity.addressables@1.16.7 --project-path .',
        f'cd {TEST_PROJECTS_DIR}/{project_folder} && unity-config project add dependency com.unity.scriptablebuildpipeline@1.11.2 --project-path .',
		f'cd {TEST_PROJECTS_DIR}/{project_folder} && unity-config project add dependency com.unity.test-framework@1.1.18 --project-path .',
        f'cd {TEST_PROJECTS_DIR}/{project_folder} && unity-config project add dependency com.unity.test-framework.performance@2.4.0 --project-path .',
		f'cd {TEST_PROJECTS_DIR}/{project_folder} && unity-config project add dependency com.unity.test-framework.utp-reporter@1.0.2-preview --project-path .',
		f'cd {TEST_PROJECTS_DIR}/{project_folder} && unity-config project add dependency com.unity.test-framework.build@0.0.1-preview.12 --project-path .',
              
		f'cd {TEST_PROJECTS_DIR}/{project_folder} && unity-config project add dependency \"com.unity.testing.graphics-performance@ssh://git@github.cds.internal.unity3d.com/unity/com.unity.testing.graphics-performance.git\"  --project-path .',        
		f'cd {TEST_PROJECTS_DIR}/{project_folder} && unity-config project add dependency \"unity.graphictests.performance.universal@ssh://git@github.cds.internal.unity3d.com/unity/unity.graphictests.performance.universal.git\" --project-path .',	
		
        f'cd {TEST_PROJECTS_DIR}/{project_folder} && unity-config project add testable com.unity.cli-project-setup  --project-path .',		
		f'cd {TEST_PROJECTS_DIR}/{project_folder} && unity-config project add testable com.unity.test.performance.runtimesettings  --project-path .',
		f'cd {TEST_PROJECTS_DIR}/{project_folder} && unity-config project add testable com.unity.test.metadata-manager  --project-path .',
        f'cd {TEST_PROJECTS_DIR}/{project_folder} && unity-config project add testable com.unity.testing.graphics-performance --project-path .',
        f'cd {TEST_PROJECTS_DIR}/{project_folder} && unity-config project add testable com.unity.render-pipelines.core  --project-path .',
        f'cd {TEST_PROJECTS_DIR}/{project_folder} && unity-config project add testable unity.graphictests.performance.universal  --project-path .',
        f'cd {TEST_PROJECTS_DIR}/{project_folder} && unity-config project set project-update false --project-path .'
    ]
    return cmds