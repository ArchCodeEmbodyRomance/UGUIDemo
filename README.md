# UGUI 实现的各种功能合集 （当前版本5.6+）
>
##1.UGUI图文混排
#####（参考链接:<a href="https://blog.csdn.net/akof1314/article/details/49028279">https://blog.csdn.net/akof1314/article/details/49028279</a>）

1.1 利用正则表达式找到所有quad标签的位置或者找到自定义图文混排格式的位置并转化为quad签并重新填入Text。

1.2 重载OnPopulateMesh方法，去除Quad标签原来的乱码，根据quad标签位置找到UGUI自动生成的quad网格位置，在网格位置的地方覆盖Image
>
##2.