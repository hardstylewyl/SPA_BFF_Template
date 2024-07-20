<script setup lang="ts">
import { useHttpClient } from '@/services/api';
import { onMounted, ref } from 'vue';

const { get, post } = useHttpClient()
const loginState = ref(false)
const userinfo = ref({})
const getUserInfoAndChangeXSRFToken = async ()=>{
  const { data } = await get("/api/Test/UserInfo")
  userinfo.value = data
}
const checkAuth = async () => {
  const { data } = await get("/api/Test/CheckAuth")
  loginState.value = data
  if (data) {
    getUserInfoAndChangeXSRFToken()
  }
}
onMounted(() => {
  checkAuth()
})

const singin = async () => {
  await post("/api/Test/Sigin?name=test")
  await checkAuth()
}
const sigout = async () => {
  await post("/api/Test/Sigout")
  await checkAuth()
}


</script>

<template>
  <div>
    <div v-if="loginState">
      登录成功
      {{ userinfo }}
      <button @click="sigout()">退出</button>
      <button @click="getUserInfoAndChangeXSRFToken()">更新XSRF-Token</button>
    </div>
    <div v-else>
      <button @click="singin()">登录</button>
      <!-- 表单提交式使用xsrf-token -->
      <!-- <form action="/api/Test/Sigin?name=test" method="post">
        <input
            type="hidden"
            id="__RequestVerificationToken"
            name="__RequestVerificationToken"
            v-bind="{value: getCookie('XSRF-RequestToken')}">
          
          </input>
        <button type="submit">登录</button>
      </form> -->
    </div>
  </div>
</template>