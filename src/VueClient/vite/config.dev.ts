import { mergeConfig, type UserConfig } from 'vite'
import checker from 'vite-plugin-checker'
import mkcert from 'vite-plugin-mkcert'
import baseConfig from './config.base'

// 开发环境
export default mergeConfig(
	<UserConfig>{
		mode: 'development',
		base: '/',
		plugins: [
			// 运行时检查类型
			checker({
				vueTsc: true,
			}),
			//ssl证书证书
			mkcert()
		],
		optimizeDeps: {
			force: true,
		},
		server: {
			https: true as unknown,
			port: 3000,
			strictPort: true, // exit if port is in use
			hmr: {
				clientPort: 3000, // point vite websocket connection to vite directly, circumventing .net proxy
			},
			open: false,
			fs: {
				// 文件打开是相对于根目录的绝对路径
				strict: true,
			},
		},
	}, baseConfig)
