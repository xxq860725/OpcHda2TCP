# OpcHda2TCP
###作用
将wincc opc hda server数据通过tcp的方式发送出去，以便于在广域网范围内可以访问到wincc的历史数据。
###为什么不用 wincc oledb 
wincc oledb是一个不错的接口，但是不知道什么原因在wincc 7.3中很不稳定，查询的时间一长就会抛出异常。可能打个update补丁会好，但是现在系统在运行，不能随意停机打补丁。
打补丁的事情还不知道延期到何时。暂时先用这个接口把数据传出去。以后其他项目需要把opc hda 发布出去时也可以用。
