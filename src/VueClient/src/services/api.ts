import axios from 'axios'

export const getCookie = (cookieName: string) => {
	const name = `${cookieName}=`;
	const decodedCookie = decodeURIComponent(document.cookie);
	const ca = decodedCookie.split(";");
	for (let i = 0; i < ca.length; i += 1) {
		let c = ca[i];
		while (c.charAt(0) === " ") {
			c = c.substring(1);
		}
		if (c.indexOf(name) === 0) {
			return c.substring(name.length, c.length);
		}
	}
	return "";
}
export const useHttpClient = () => {
	const axiosIns = axios.create()
	// 请求拦截器
	axiosIns.interceptors.request.use(config => {

		//config.headers.Authorization = localStorage.getItem('token') || ''

		// 配置xsrf token
		config.withXSRFToken = true
		config.xsrfHeaderName = 'X-XSRF-TOKEN'
		config.xsrfCookieName = 'XSRF-RequestToken'
		return config
	}, error => {
		return Promise.reject(error)
	})

	// 响应拦截器
	axiosIns.interceptors.response.use(response => {
		return response
	})

	return {
		get: axiosIns.get,
		post: axiosIns.post,
	}
}

