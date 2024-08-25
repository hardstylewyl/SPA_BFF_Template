<script setup lang="ts">
import { useHttpClient } from '@/services/api';
import { onMounted, ref } from 'vue';

const { get, post } = useHttpClient()
const loginState = ref(false)
const userinfo = ref({})
const callResult = ref('')
const getUserInfoAndChangeXSRFToken = async () => {
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

const singin = () => {
    window.location.href = '/Account/Login'
}
const sigout = async () => {
    window.location.href = '/Account/Logout'
}

const callApi = async () => {
    await get("/res/Student/Hello")
        .then(({ data }) => {
            callResult.value = "/res/Student/Hello" + "\r\n" + JSON.stringify(data)
        })
        .catch(err => {
            callResult.value = "/res/Student/Hello" + "\r\n" + JSON.stringify(err)
        })
}
const callApi2 = async () => {
    await get("/res/Student/GetStudents")
        .then(({ data }) => {
            callResult.value = "/res/Student/GetStudents" + "\r\n" + JSON.stringify(data)
        })
        .catch(err => {
            callResult.value = "/res/Student/GetStudents" + "\r\n" + JSON.stringify(err)
        })
}
const callApi3 = async () => {
    await post("/res/Student/CreateStudent", {
        name: "test",
    })
        .then(({ data }) => {
            callResult.value = "/res/Student/CreateStudent" + "\r\n" + JSON.stringify(data)
        })
        .catch(err => {
            callResult.value = "/res/Student/CreateStudent" + "\r\n" + JSON.stringify(err)
        })

}
const callApi4 = async () => {
    await get("/res/Student/DeleteStudent?id=")
        .then(({ data }) => {
            callResult.value = "/res/Student/DeleteStudent" + "\r\n" + JSON.stringify(data)
        })
        .catch(err => {
            callResult.value = "/res/Student/DeleteStudent" + "\r\n" + JSON.stringify(err)
        })
}


</script>
<template>
    <div>
        <div>BffAccessApi</div>
        <div v-if="loginState">
            <div>userInfo:{{ userinfo }}</div>
            <button @click="sigout()">登出</button>
            <button @click="callApi()">调用Hello（无需权限）</button>
            <button @click="callApi2()">获取所有学生(需要权限)</button>
            <button @click="callApi3()">创建学生(需要权限)</button>
            <button @click="callApi4()">删除学生(需求权限)</button>
            <div>
                <h3>调用结果</h3>
                <textarea v-text="callResult" style="height: 60vh;width: 80vw;"></textarea>
            </div>
        </div>
        <button v-else @click="singin()">登录</button>

    </div>
</template>