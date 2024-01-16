# 一、软件介绍：
 
     崇令仓储相关系统，均由C#语言编写，当前文档针对WMS-BS版本进行说明，其它相关开源软件下载或说明可访问：
     
     WMS-BS(独立WMS网页版)：https://github.com/243480503/WMS-BS
     WCS-BS(独立WCS网页版)：https://github.com/243480503/WCS-BS
     WCS-WPF(独立WCS桌面版)：https://github.com/243480503/WCS-WPF
     WMS与WCS二合一(同时包含WMS与WCS的网页版)：https://github.com/243480503/WMS-WCS
     
# 二、适用场景
     
     WMS-BS包含手持PDA相关功能，适用于利用智能设备进行作业的仓库，主要实现出库、入库、质检、盘点、报表 等功能。
     仓储智能设备包含但不限于：输送线、机械手、堆垛机、AGV、穿梭车 等
 
# 三、目录文件说明
    
      Project ：包含整套C#源码，如果您是开发者，可下载该目录
      发布 ：已编译的文件，可直接部署到IIS站点，如果您不熟悉代码，可下载该目录    
      数据库：文件较大，采用分卷压缩。此数据库包含2年的使用数据（注：该数据库已脱敏）
      
      三个目录压缩后下载大小为：1.22GB      

# 四、软件环境

      开发工具：Microsoft Visual Studio Enterprise 2019 及以上
      数据库  ：Microsoft SQL Server 2019 及以上
      运行环境：.Net Framework 4.8 及以上
      
# 五、软件部署

      下载压缩包解压后，存在Project、发布、数据库 三个文件夹，以下内容只针对  （发布+数据库） 方案，
      如果您选择 （Project+数据库） 方案，则默认您已具备一定的开发能力，可跳过该步骤。

   ## 1、安装运行环境
         
         a>安装 .Net Framework 4.8

   ## 2、还原数据库

         a>解压数据库文件夹中的ChongLing.part01或ChongLing.part02，得到数据库备份文件ChongLing.bak
         
         b>登录数据库，在数据库上右键点击“还原文件和文件组”，目标数据库填“ChongLing”

         c>新建数据库登录名：wms，密码：123456，并将该用户映射到“ChongLing”数据库，修改权限为“Owner”。

   ## 3、部署站点
         
         a>打开IIS，新建网站，网站映射目录为“发布”，指定一个访问端口，如：3000。

   ## 4、访问系统
         
         a>若服务器IP为192.168.1.36,则浏览器（最好为谷歌浏览器）访问地址 http://192.168.1.36:3000，
           打开后能看见登录页面，则表示部署成功。
     
   ## 5、登录系统
     
         a>拥有“一般管理员”权限的用户名：0184，密码：123
           拥有“超级管理员”权限的用户名：admin，密码：a123456，该用户有变更系统固有参数的开发权限，若
           您对系统尚不熟悉，不建议使用admin账号对数据做任何更改，以免造成数据异常。

   ## 6、手持PDA的部署

         a>登录到系统后，点击右上角个人图标，出现下拉菜单，点击“手持下载”按钮，即可下载 “PDA.apk”文件，
           该文件为安卓系统手持设备的安装文件。
     
   ## 7、手持PDA的配置

         a>安装“PDA.apk”后，初次打开该程序时，会提示您填入“服务器IP”与“端口”，其中服务器IP即IIS站点IP，
           如“192.168.1.36”（不可填写localhost），端口即之前IIS设置的3000。
           若卸载该apk，再次安装需重新配置。

         b>点击保存后，若配置正确，则可看见手持的登录界面，登录界面用户名与密码，与PC端的用户名与密码相同。

# 六、系统配置
     
    1、若想更改数据库地址与端口，请前往文件目录 /Configs/database.config 进行修改
    
    2、若想更改软件名称、图标、联系方式、站点链接 等信息，请前往文件目录 /Configs/system.config 进行修改
    
# 七、接口配置
     
     1、ERP/MES 接口配置
        以admin账号登录，进入主页面，点击“系统管理”-“数据字典”-“业务配置”-“接口”-“ERP”，将“值”中
        的内容改为ERP/MES的接口地址即可，此处更改需重启IIS后生效。
      
     2、WCS接口配置
        以admin账号登录，进入主页面，点击“系统管理”-“数据字典”-“业务配置”-“接口”-“WCS”，将“值”中
        的内容改为WCS的接口地址即可，此处更改需重启IIS后生效。

     注：此处ERP/MES接口，和WCS接口，均为外部接口，考虑到部分用户暂时没有外部接口可用，
         可将“值”配置为“”或“http://”，程序会默认调用外部接口都是成功的，即便接口不存在。
         这样做的目的是让整个流程可以走通。
        
# 八、相关截图
 
     <br/><br/><img src="https://github.com/243480503/WMS-BS/blob/main/%E7%9B%B8%E5%85%B3%E6%88%AA%E5%9B%BE/PC_Logo.jpg" width="60%" height="60%" ></img>
     <br/><br/><div style='text-align:center;'>登录页面</div><br/><br/>

     <br/><br/><img src="https://github.com/243480503/WMS-BS/blob/main/%E7%9B%B8%E5%85%B3%E6%88%AA%E5%9B%BE/PC_Index.jpg" width="60%" height="60%" ></img>
     <br/><br/><div style='text-align:center;'>主页面</div><br/><br/>

     <br/><br/><img src="https://github.com/243480503/WMS-BS/blob/main/%E7%9B%B8%E5%85%B3%E6%88%AA%E5%9B%BE/PC_InBound.jpg" width="60%" height="60%" ></img>
     <br/><br/><div style='text-align:center;'>入库页面</div><br/><br/>
     ![Image](https://github.com/243480503/WMS-BS/blob/main/%E7%9B%B8%E5%85%B3%E6%88%AA%E5%9B%BE/PC_InBound.jpg)
     ![image](https://github.com/243480503/WMS-BS/blob/main/%E7%9B%B8%E5%85%B3%E6%88%AA%E5%9B%BE/PC_InBound.jpg)
# 九、其它说明
 
    当前软件遵循MIT开源协议，您可对软件进行更改并使用。
    
    若需反馈bug，或有其它疑问，或有定制需求，均可联系18718690940（微信同号），
    或加入技术交流QQ群795702839。

    
