import React, { Component } from 'react';
import { TouchableOpacity } from 'react-native';
import { StackNavigator} from 'react-navigation'
import HomeScreen from './HomeScreen';


const stackNav = StackNavigator({
    Home: {
        screen: HomeScreen,
        navigationOptions:({navigation}) => ({
            // headerLeft:(
            //   <TouchableOpacity onPress={() => navigation.navigate("DrawerOpen")}>
            //     <IOSIcon name="ios-menu" size={30} />
            //   </TouchableOpacity>
            // ),
            headerStyle: { paddingRight: 10, paddingLeft: 10 }
        })
    }
})

export default stackNav;