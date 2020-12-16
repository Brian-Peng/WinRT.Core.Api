1、启用Swagger
2、Swagger开启加权小锁，采用JWT认证
3、Swagger配合使用API多版本
4、解决swagger文档项目名称不显示bug

2020-12-6
1、把解决方案和项目放在同一个目录下
2、把本地项目推送到github上
3、获取token令牌
4、swagger中开启jwt服务
5、API接口授权策略
6、配置认证服务
7、配置官方认证中间件


1、开发JWT流程：
		授权
		定义认证方案
		生成token令牌（用户输入账号密码登入成功之后，返回给用户一个token）
		开启验证


结合数据库动态分配接口
JWT授权认证流程——官方认证 / (这里采用)自定义中间件。
1、把验证策略写到数据库，将接口地址和角色授权分离
├── Module                      　　  　     // 菜单表
├── ModulePermission                        // 菜单与按钮关系表
├── Permission                              // 按钮表 
├── Role                                    // 角色表
├── RoleModulePermission                    // 按钮跟权限关联表
├── UserRole                                // 用户跟角色关联表
└── sysUserInfo                             // 用户信息表


框架模块：
	泛型仓储+服务+接口
	异步 async/await 开发；
	接入国产数据库ORM组件 —— SqlSugar，封装数据库操作；

	- 创建实体Model数据层(WinRT.Core.Model)
		models:存放的是整个项目的数据库表实体类
		VeiwModels:是存放的DTO实体类
	-设计仓储接口与其实现类
		repository就是一个管理数据持久层的，它负责数据的CRUD(Create, Read, Update, Delete) ,service layer是业务逻辑层，它常常需要访问repository层。
		仓储模式的基本就是如何将持久化动作和对象获取方式以及领域模型Domain Model结合起来，进一步：如何更加统一我们的语言(Ubiquitous Language)，一个整合持久化技术的好办法是仓储Repositories。
		为什么要单独在仓储层来引入ORM持久化接口，是因为，降低耦合，如果以后想要换成EF或者Deper，只需要修改Repository就行了，其他都不需要修改，达到很好的解耦效果。
		很多人都有一个这样的想法，仓储就是Dal层？我这里简单说一下两者的区别（我Blog.Core项目可能不是体现的出来，DDD可能更好理解）：
		1、仓储是一种模式，可以单建一层，也可以放到基础设施层，

			  而Dal是三层架构种的某一层，是一个架构

		2、仓储官方的定义，他是一个管理者，去管理实体对象和ORM映射对象的一个集合，是ORM操作db，

			  而Dal就是完完全全的操作db数据库了，

			  请注意，不要把 Sqlsugar 当成三层中的 sqlhelper，不一样！     

		3、仓储更多的是一个聚合，在下一个系列DDD中，我们说到了一个概念，一个聚合的划分其实就是一个仓储。

		4、然后我们进一步细化，一个仓储 = 一个聚合 = 有一个聚合根 = 一个微服务！

			 多个微服务之间交互，就是通过聚合根来的。

		首先什么是ORM， 对象关系映射（Object Relational Mapping，简称ORM）模式是一种为了解决面向对象与关系数据库存在的互不匹配的现象的技术。简单的说，ORM是通过使用描述对象和数据库之间映射的元数据，将程序中的对象自动持久化到关系数据库中。

	 - 设计服务接口与其实现类
		 创建 WinRT.Core.IServices 和 WinRT.Core.Services 业务逻辑层，就是和我们平时使用的三层架构中的BLL层很相似：Service层只负责将Repository仓储层的数据进行调用，至于如何是与数据库交互的，它不去管，这样就可以达到一定程度上的解耦，假如以后数据库要换，比如MySql，那Service层就完全不需要修改即可，至于真正意义的解耦，还是得靠依赖注入。


