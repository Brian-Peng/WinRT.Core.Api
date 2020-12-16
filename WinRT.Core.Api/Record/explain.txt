开发基于.netCore Api 3.1后台框架

功能与进度:

	框架模块：
	- 采用泛型仓储+服务+接口的形式封装框架
	- 异步 async/await 开发
	- 接入国产数据库ORM组件 —— SqlSugar，封装数据库操作

	组件模块：
	- 使用 Swagger 做api文档，并提供api版本控制；
	- 使用 Automapper 处理对象映射；
	- 使用 AutoFac 做依赖注入容器，并提供批量服务注入
	- 支持 CORS 跨域；
	- 封装 JWT 自定义策略授权；
	- AOP拦截器: aop处理日志、aop内存缓存(采用缓存特性)

	物理模型：
	├── Module                      　　
	├── ModulePermission                      
	├── Permission                   
	├── Role                            
	├── RoleModulePermission              
	├── UserRole                             
	└── sysUserInfo      
	└── OperateLog  


遗漏部分：
C.6、JWT滑动刷新
E.2、原生依赖注入一对多
E.5、Autofac组件使用（视频）
E.6、依赖注入直播（视频）