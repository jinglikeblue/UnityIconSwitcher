//切换App图标
void SwitchAppIcon(const char *name){
    NSString *s = [NSString stringWithUTF8String:name];
    NSLog(@"[Icon Switcher] 更换 App Icon: %@", s);
    
    if([s isEqualToString:@""]){
        s = nil;
    }
    
    [[UIApplication sharedApplication] setAlternateIconName:s completionHandler:^(NSError * _Nullable error) {
        if(error){
            NSLog(@"[Icon Switcher] 更换 App Icon 发生了错误 : %@", error);
        }else{
            NSLog(@"[Icon Switcher] 更换 App Icon 成功");
        }
    }];
}


#import <objc/runtime.h>

@implementation UIViewController (Present)


+ (void)load {
    
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        
        Method presentM = class_getInstanceMethod(self.class,@selector(presentViewController:animated:completion:));
        Method presentSwizzlingM = class_getInstanceMethod(self.class,@selector(dy_presentViewController:animated:completion:));
        method_exchangeImplementations(presentM, presentSwizzlingM);

    });
}

- (void)dy_presentViewController:(UIViewController*)viewControllerToPresent animated:(BOOL)flag completion:(void(^)(void))completion {
    
    if ([viewControllerToPresent isKindOfClass:[UIAlertController class]]) {
        
        UIAlertController *alertController = (UIAlertController*)viewControllerToPresent;
        
        if (alertController.title == nil && alertController.message == nil) {
            
            NSLog(@"[Icon Switcher] 替换 App Icon 操作");
            return;
            
        } else {
            
            [self dy_presentViewController:viewControllerToPresent animated:flag completion:completion];
            return;
            
        }
        
    }
    
    [self dy_presentViewController:viewControllerToPresent animated:flag completion:completion];
    
}

@end
