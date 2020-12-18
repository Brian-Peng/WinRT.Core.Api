# WinRT.Core.Api
 
#基于.netCore Api 3.1后台框架

功能与进度:
 框架模块
- 采用泛型仓储+服务+接口的形式封装框架
- 异步 async/await 开发
- 接入国产数据库ORM组件 —— SqlSugar，封装数据库操作

 组件模块
  - 使用Swagger做api文档，并提供api版本控制
  - 使用Automapper处理对象映射
  - 使用AutoFac 做依赖注入容器，并提供批量服务注入
  - 支持CORS跨域
  - 封装JWT自定义策略授权
  - AOP拦截器：AOP处理日志、AOP内存缓存（采用缓存特性）
  - 提供 Redis 做缓存处理
